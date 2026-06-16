using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tesseract;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CabinetController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CabinetController(ApplicationDbContext db, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetClientId()
        {
            var claim = User.FindFirst("ClientId")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home", new { area = "" });

                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }

            var client = await _db.Clients
                .Include(c => c.Cars)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }

            var orders = await _db.Orders
                .Include(o => o.Car)
                .Include(o => o.Master)
                .Where(o => o.Car!.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.Orders = orders;
            ViewData["Title"] = "Личный кабинет";
            return View("Dashboard/Index");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home", new { area = "" });

                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }

            var orders = await _db.Orders
                .Include(o => o.Car)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks!)
                    .ThenInclude(cw => cw.Service)
                .Where(o => o.Car!.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewData["Title"] = "Мои заказы";
            return View("Orders/Orders", orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var clientId = GetClientId();
            var order = await _db.Orders
                .Include(o => o.Car)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks!)
                    .ThenInclude(cw => cw.Service)
                .FirstOrDefaultAsync(o => o.OrderNumber == id && o.Car!.ClientId == clientId);

            if (order == null) return NotFound();

            ViewData["Title"] = $"Заказ #{id}";
            return PartialView("Orders/_OrderDetails", order);
        }

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            if (GetClientId() == 0)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home", new { area = "" });

                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }

            var services = await _db.Services
                .Where(s => !s.IsConsultation)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();
            ViewBag.Services = services;

            return View("Orders/Book");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int carId, List<int>? serviceCodes, bool hasOther, string description, DateTime? scheduledAt)
        {
            var clientId = GetClientId();
            var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == carId && c.ClientId == clientId);
            if (car == null) return BadRequest();

            var order = new Order
            {
                CarId = carId,
                OrderDate = PermTime.Now,
                ScheduledAt = scheduledAt
            };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            if (serviceCodes != null && serviceCodes.Any())
            {
                foreach (var sc in serviceCodes)
                {
                    _db.CompletedWorks.Add(new CompletedWork
                    {
                        OrderNumber = order.OrderNumber,
                        ServiceCode = sc,
                        WheelCount = 4,
                        CompletionTimeMin = 0,
                        WorkTotal = 0
                    });
                }
                await _db.SaveChangesAsync();

                TempData["Success"] = "Запись создана! Мы свяжемся с вами для подтверждения.";
            }

            if (hasOther || (serviceCodes == null || !serviceCodes.Any()))
            {
                var consultation = await _db.Services.FirstOrDefaultAsync(s => s.IsConsultation);
                if (consultation == null)
                {
                    consultation = new Service
                    {
                        ServiceName = "Консультация",
                        ServiceCost = 0,
                        IsConsultation = true
                    };
                    _db.Services.Add(consultation);
                    await _db.SaveChangesAsync();
                }

                _db.CompletedWorks.Add(new CompletedWork
                {
                    OrderNumber = order.OrderNumber,
                    ServiceCode = consultation.ServiceCode,
                    WheelCount = 0,
                    CompletionTimeMin = 0,
                    WorkTotal = 0
                });
                await _db.SaveChangesAsync();

                TempData["Success"] = hasOther
                    ? "Запись создана! Мы свяжемся с вами для подтверждения."
                    : "Запись на консультацию создана! Мастер свяжется с вами.";
            }

            return RedirectToAction("Orders");
        }

        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var clientId = GetClientId();
            var order = await _db.Orders
                .Include(o => o.Car)
                .Include(o => o.CompletedWorks!)
                    .ThenInclude(cw => cw.Service)
                .FirstOrDefaultAsync(o => o.OrderNumber == id && o.Car!.ClientId == clientId);

            if (order == null) return NotFound();
            if (order.Status != "Новый") return BadRequest("Редактирование возможно только для новых заказов");

            var services = await _db.Services
                .Where(s => !s.IsConsultation)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            ViewBag.Services = services;
            ViewData["Title"] = $"Редактировать заказ #{id}";
            return View("Orders/Edit", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, List<int>? serviceCodes, bool hasOther, string description, DateTime? scheduledAt)
        {
            var clientId = GetClientId();
            var order = await _db.Orders
                .Include(o => o.CompletedWorks)
                .FirstOrDefaultAsync(o => o.OrderNumber == id && o.Car!.ClientId == clientId);

            if (order == null) return NotFound();
            if (order.Status != "Новый") return BadRequest("Редактирование возможно только для новых заказов");

            order.ScheduledAt = scheduledAt;

            if (order.CompletedWorks != null)
            {
                _db.CompletedWorks.RemoveRange(order.CompletedWorks);
                await _db.SaveChangesAsync();
            }

            if (serviceCodes != null && serviceCodes.Any())
            {
                foreach (var sc in serviceCodes)
                {
                    _db.CompletedWorks.Add(new CompletedWork
                    {
                        OrderNumber = order.OrderNumber,
                        ServiceCode = sc,
                        WheelCount = 4,
                        CompletionTimeMin = 0,
                        WorkTotal = 0
                    });
                }
                await _db.SaveChangesAsync();
            }

            if (hasOther || (serviceCodes == null || !serviceCodes.Any()))
            {
                var consultation = await _db.Services.FirstOrDefaultAsync(s => s.IsConsultation);
                if (consultation == null)
                {
                    consultation = new Service
                    {
                        ServiceName = "Консультация",
                        ServiceCost = 0,
                        IsConsultation = true
                    };
                    _db.Services.Add(consultation);
                    await _db.SaveChangesAsync();
                }

                _db.CompletedWorks.Add(new CompletedWork
                {
                    OrderNumber = order.OrderNumber,
                    ServiceCode = consultation.ServiceCode,
                    WheelCount = 0,
                    CompletionTimeMin = 0,
                    WorkTotal = 0
                });
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Заказ обновлён!";
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var clientId = GetClientId();
            var order = await _db.Orders
                .Include(o => o.CompletedWorks)
                .FirstOrDefaultAsync(o => o.OrderNumber == id && o.Car!.ClientId == clientId);

            if (order == null) return Json(new { success = false, error = "Заказ не найден" });

            if (order.Status != "Новый")
                return Json(new { success = false, error = "Нельзя отменить заказ, который уже обрабатывается" });

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCars()
        {
            var clientId = GetClientId();
            var cars = await _db.Cars
                .Where(c => c.ClientId == clientId)
                .Select(c => new { c.CarId, c.FullInfo })
                .ToListAsync();

            return Json(cars);
        }

        [HttpGet]
        public IActionResult AddCar()
        {
            if (GetClientId() == 0)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home", new { area = "" });

                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }
            ViewData["Title"] = "Добавить автомобиль";
            return View("Cars/AddCar");
        }

        [HttpPost]
        public async Task<IActionResult> ScanDocument(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return Json(new { error = "Файл не выбран" });

            try
            {
                using var ms = new MemoryStream();
                await photo.CopyToAsync(ms);
                var base64Image = Convert.ToBase64String(ms.ToArray());

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(120);

                var response = await client.PostAsync(
                    "http://localhost:5003/ocr",
                    JsonContent.Create(new { base64_image = base64Image }));

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return Json(new { error = $"OCR service error: {response.StatusCode}", details = responseBody });

                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement.GetProperty("text").GetString() ?? "";
                Console.WriteLine($"[DEBUG] OCR text: {text[..Math.Min(500, text.Length)]}");

                var result = ParseOcrText(text);
                return Json(result);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message;
                var msg = "Ошибка распознавания: " + ex.Message + (inner != null ? " (" + inner + ")" : "");
                return Json(new { error = msg });
            }
        }

        private static object ParseOcrText(string text)
        {
            var lines = text.Split('\n');
            var lower = text.ToLower();

            var brands = new[]
            {
                "toyota","bmw","mercedes","audi","volkswagen","vw","opel","ford","renault","peugeot","citroen",
                "hyundai","kia","honda","nissan","mazda","mitsubishi","subaru","suzuki","lexus","infiniti",
                "lada","vaz","uaz","gaz","chevrolet","cadillac","tesla","volvo","skoda","seat","fiat","ferrari",
                "porsche","land rover","jaguar","mini","chrysler","dodge","jeep","bentley","aston martin",
                "тойота","бмв","мерседес","ауди","фольксваген","опель","форд","рено","пежо","ситроен",
                "хёндэ","хендай","киа","хонда","ниссан","мазда","митсубиси","субару","сузуки","лексус","инфинити",
                "лада","ваз","уаз","газ","шевроле","кадиллак","тесла","вольво","шкода","сеат","фиат","феррари",
                "порш","ланд ровер","ягуар","мини","крайслер","додж","джип","бентли","астон мартин"
            };

            var brand = "";
            foreach (var b in brands)
            {
                if (lower.Contains(b))
                {
                    brand = b;
                    break;
                }
            }

            var model = "";
            if (!string.IsNullOrEmpty(brand))
            {
                var brandIdx = Array.FindIndex(lines, l => l.ToLower().Contains(brand));
                if (brandIdx >= 0)
                {
                    for (int i = brandIdx; i < Math.Min(brandIdx + 5, lines.Length); i++)
                    {
                        var line = lines[i].Trim();
                        if (line.Length > 2 && line.Length < 35 && !char.IsDigit(line[0]) && !line.ToLower().Contains(brand))
                        {
                            model = line;
                            break;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(model))
            {
                var modelMatch = Regex.Match(text, @"(?:наименование|модель|model)\s*[:\s]\s*(.+)", RegexOptions.IgnoreCase);
                if (modelMatch.Success)
                    model = modelMatch.Groups[1].Value.Trim();
            }

            var vin = "";
            var vinMatch = Regex.Match(text, @"\b[A-HJ-NPR-Z0-9]{17}\b", RegexOptions.IgnoreCase);
            if (vinMatch.Success)
                vin = vinMatch.Value.ToUpper();
            if (string.IsNullOrEmpty(vin))
            {
                var vinSection = Regex.Match(text, @"(?:VIN|vin|номер)\s*[:\s]\s*([A-Z0-9]{13,20})", RegexOptions.IgnoreCase);
                if (vinSection.Success)
                    vin = vinSection.Groups[1].Value.ToUpper();
            }

            var licensePlate = "";
            var plateMatch = Regex.Match(text, @"[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}", RegexOptions.IgnoreCase);
            if (plateMatch.Success)
                licensePlate = plateMatch.Value.ToUpper();

            var year = "";
            foreach (Match y in Regex.Matches(text, @"\b(19[0-9]{2}|20[0-9]{2})\b"))
            {
                if (int.TryParse(y.Value, out var yv) && yv >= 1990 && yv <= 2030)
                {
                    if (string.IsNullOrEmpty(year) || yv > int.Parse(year))
                        year = yv.ToString();
                }
            }

            if (!string.IsNullOrEmpty(brand))
                brand = char.ToUpper(brand[0]) + brand[1..];

            return new
            {
                brand,
                model,
                year,
                vin,
                licensePlate
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCar(Car car, List<IFormFile>? photos)
        {
            var clientId = GetClientId();
            if (clientId == 0)
            {
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Index", "Home", new { area = "" });

                return RedirectToAction("Login", "Auth", new { area = "Customer" });
            }

            car.ClientId = clientId;

            if (ModelState.IsValid)
            {
                if (await _db.Cars.AnyAsync(c => c.LicensePlate == car.LicensePlate))
                    ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                if (await _db.Cars.AnyAsync(c => c.Vin == car.Vin))
                    ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");

                if (ModelState.IsValid)
                {
                    if (photos != null && photos.Count > 0)
                    {
                        var photoPaths = new List<string>();
                        foreach (var photo in photos)
                        {
                            if (photo != null && photo.Length > 0)
                            {
                                var photoPath = await SavePhotoAsync(photo);
                                photoPaths.Add(photoPath);
                            }
                        }
                        if (photoPaths.Count > 0)
                        {
                            car.PhotoPath = photoPaths[0];
                            if (photoPaths.Count > 1)
                            {
                                car.AdditionalPhotos = System.Text.Json.JsonSerializer.Serialize(photoPaths.Skip(1).ToList());
                            }
                        }
                    }

                    _db.Cars.Add(car);
                    await _db.SaveChangesAsync();

                    TempData["Success"] = "Автомобиль добавлен!";
                    return RedirectToAction("Index");
                }
            }

            ViewData["Title"] = "Добавить автомобиль";
            return View("Cars/AddCar", car);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var clientId = GetClientId();
            var car = await _db.Cars
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);
            if (car == null) return NotFound();

            ViewData["Title"] = car.Brand + " " + car.Model;
            return View("Cars/Details", car);
        }

        [HttpGet]
        public async Task<IActionResult> EditCar(int id)
        {
            var clientId = GetClientId();
            var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);
            if (car == null) return NotFound();

            ViewData["Title"] = "Редактировать автомобиль";
            return View("Cars/EditCar", car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCar(Car car, List<IFormFile>? photos)
        {
            var clientId = GetClientId();
            var existing = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == car.CarId && c.ClientId == clientId);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _db.Cars.AnyAsync(c => c.LicensePlate == car.LicensePlate && c.CarId != car.CarId))
                    ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                if (await _db.Cars.AnyAsync(c => c.Vin == car.Vin && c.CarId != car.CarId))
                    ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");

                if (ModelState.IsValid)
                {
                    existing.Brand = car.Brand;
                    existing.Model = car.Model;
                    existing.ManufactureYear = car.ManufactureYear;
                    existing.LicensePlate = car.LicensePlate;
                    existing.Vin = car.Vin;

                    if (photos != null && photos.Count > 0 && photos.Any(p => p != null && p.Length > 0))
                    {
                        if (!string.IsNullOrEmpty(existing.PhotoPath))
                            DeletePhoto(existing.PhotoPath);
                        if (!string.IsNullOrEmpty(existing.AdditionalPhotos))
                        {
                            var addPhotos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(existing.AdditionalPhotos) ?? new List<string>();
                            foreach (var p in addPhotos)
                            {
                                if (!string.IsNullOrEmpty(p))
                                    DeletePhoto(p);
                            }
                        }

                        var photoPaths = new List<string>();
                        foreach (var photo in photos)
                        {
                            if (photo != null && photo.Length > 0)
                            {
                                var photoPath = await SavePhotoAsync(photo);
                                photoPaths.Add(photoPath);
                            }
                        }
                        if (photoPaths.Count > 0)
                        {
                            existing.PhotoPath = photoPaths[0];
                            existing.AdditionalPhotos = photoPaths.Count > 1
                                ? System.Text.Json.JsonSerializer.Serialize(photoPaths.Skip(1).ToList())
                                : null;
                        }
                    }

                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                    {
                        var pgEx = (PostgresException)ex.InnerException;
                        if (pgEx.ConstraintName == "IX_Cars_LicensePlate")
                            ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                        else if (pgEx.ConstraintName == "IX_Cars_Vin")
                            ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");
                        ViewData["Title"] = "Редактировать автомобиль";
                        return View("Cars/EditCar", car);
                    }

                    TempData["Success"] = "Автомобиль обновлён!";
                    return RedirectToAction("Index");
                }
            }

            ViewData["Title"] = "Редактировать автомобиль";
            return View("Cars/EditCar", car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var clientId = GetClientId();
            var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);
            if (car == null) return NotFound();

            _db.Cars.Remove(car);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Автомобиль удалён!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CarsPartial()
        {
            var clientId = GetClientId();
            if (clientId == 0) return Unauthorized();

            var cars = await _db.Cars
                .Where(c => c.ClientId == clientId)
                .ToListAsync();

            return PartialView("Cars/_CarsList", cars);
        }

        [HttpGet]
        public async Task<IActionResult> OrdersPartial()
        {
            var clientId = GetClientId();
            if (clientId == 0) return Unauthorized();

            var orders = await _db.Orders
                .Include(o => o.Car)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks!)
                    .ThenInclude(cw => cw.Service)
                .Where(o => o.Car!.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return PartialView("Orders/_OrdersList", orders);
        }

        private async Task<string> SavePhotoAsync(IFormFile photoFile)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(fileStream);
            }

            return $"/uploads/cars/{fileName}";
        }

        private void DeletePhoto(string photoPath)
        {
            if (!string.IsNullOrEmpty(photoPath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, photoPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
        }
    }
}
