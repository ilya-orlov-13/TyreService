using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления автомобилями в системе шиномонтажа.
    /// Обеспечивает CRUD-операции для сущности Car, включая загрузку фотографий.
    /// </summary>
    [Authorize(Roles = "Admin,Owner")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера CarsController.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="webHostEnvironment">Окружение веб-хоста для работы с файловой системой.</param>
        public CarsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Отображает список всех автомобилей с информацией о владельцах.
        /// </summary>
        /// <returns>Представление со списком автомобилей.</returns>
        /// <remarks>
        /// Метод загружает автомобили вместе с информацией о клиентах
        /// для отображения в таблице.
        /// </remarks>
        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.Client)
                .ToListAsync();
            
            return View(cars);
        }

        /// <summary>
        /// Отображает детальную информацию об автомобиле по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля (nullable).</param>
        /// <returns>
        /// - Представление Details с информацией об автомобиле, если найден.
        /// - NotFound (404), если автомобиль не найден или id равен null.
        /// </returns>
        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.CarId == id);
                
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        /// <summary>
        /// Отображает форму для создания нового автомобиля.
        /// </summary>
        /// <returns>Представление Create с пустой формой.</returns>
        /// <remarks>
        /// Заполняет ViewBag.ClientId списком клиентов для выпадающего списка.
        /// </remarks>
        // GET: Cars/Create
        public IActionResult Create()
        {
            ViewBag.ClientId = new SelectList(_context.Clients, "ClientId", "FullName");
            return View();
        }
        
        /// <summary>
        /// Обрабатывает данные формы для создания нового автомобиля.
        /// </summary>
        /// <param name="car">Объект Car с данными из формы.</param>
        /// <returns>
        /// - Перенаправление на Index при успешном создании.
        /// - Представление Create с ошибками валидации при неудаче.
        /// </returns>
        /// <remarks>
        /// Метод обрабатывает загрузку фотографии автомобиля и сохраняет
        /// относительный путь к файлу в свойстве PhotoPath.
        /// </remarks>
        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Brand,Model,ManufactureYear,LicensePlate,Vin")] Car car, List<IFormFile> photos)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Cars.AnyAsync(c => c.LicensePlate == car.LicensePlate))
                    ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                if (await _context.Cars.AnyAsync(c => c.Vin == car.Vin))
                    ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");

                if (ModelState.IsValid)
                {
                    // Обработка загрузки фото
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

                    _context.Add(car);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.ClientId = new SelectList(_context.Clients, "ClientId", "FullName", car.ClientId);
            return View(car);
        }

        /// <summary>
        /// Отображает форму редактирования существующего автомобиля.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля для редактирования (nullable).</param>
        /// <returns>
        /// - Представление Edit с данными автомобиля, если найден.
        /// - NotFound (404), если автомобиль не найден или id равен null.
        /// </returns>
        /// <remarks>
        /// Заполняет ViewBag.ClientId списком клиентов с текущим выбранным клиентом.
        /// </remarks>
        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            ViewBag.ClientId = new SelectList(_context.Clients, "ClientId", "FullName", car.ClientId);
            return View(car);
        }

        /// <summary>
        /// Обрабатывает данные формы для обновления информации об автомобиле.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля из маршрута.</param>
        /// <param name="car">Объект Car с обновленными данными.</param>
        /// <returns>
        /// - Перенаправление на Index при успешном обновлении.
        /// - Представление Edit с ошибками валидации при неудаче.
        /// - NotFound (404), если идентификаторы не совпадают.
        /// </returns>
        /// <remarks>
        /// Метод обновляет фотографию автомобиля при необходимости:
        /// удаляет старый файл и сохраняет новый.
        /// </remarks>
        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CarId,ClientId,Brand,Model,ManufactureYear,LicensePlate,Vin,PhotoPath,AdditionalPhotos")] Car car, List<IFormFile> photos, string? removedPhotos)
        {
            if (id != car.CarId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _context.Cars.AnyAsync(c => c.LicensePlate == car.LicensePlate && c.CarId != id))
                    ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                if (await _context.Cars.AnyAsync(c => c.Vin == car.Vin && c.CarId != id))
                    ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");

                if (ModelState.IsValid)
                {
                try
                {
                    // Получаем текущие данные из базы
                    var existingCar = await _context.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.CarId == id);
                    
                    // Обрабатываем удаленные фото
                    var photosToRemove = new List<string>();
                    if (!string.IsNullOrEmpty(removedPhotos))
                    {
                        photosToRemove = System.Text.Json.JsonSerializer.Deserialize<List<string>>(removedPhotos) ?? new List<string>();
                        
                        // Удаляем файлы с диска
                        foreach (var photoPath in photosToRemove)
                        {
                            DeletePhoto(photoPath);
                        }
                    }
                    
                    // Получаем текущие фото, исключая удаленные
                    var currentPhotos = new List<string>();
                    
                    // Добавляем основное фото, если оно не удалено
                    if (!string.IsNullOrEmpty(existingCar?.PhotoPath) && !photosToRemove.Contains(existingCar.PhotoPath))
                    {
                        currentPhotos.Add(existingCar.PhotoPath);
                    }
                    
                    // Добавляем дополнительные фото, исключая удаленные
                    if (!string.IsNullOrEmpty(existingCar?.AdditionalPhotos))
                    {
                        try
                        {
                            var existingAdditionalPhotos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(existingCar.AdditionalPhotos);
                            if (existingAdditionalPhotos != null)
                            {
                                currentPhotos.AddRange(existingAdditionalPhotos.Where(p => !photosToRemove.Contains(p)));
                            }
                        }
                        catch
                        {
                            // Игнорируем ошибки десериализации
                        }
                    }
                    
                    // Добавляем новые загруженные фото
                    if (photos != null && photos.Count > 0)
                    {
                        foreach (var photo in photos)
                        {
                            if (photo != null && photo.Length > 0)
                            {
                                var photoPath = await SavePhotoAsync(photo);
                                currentPhotos.Add(photoPath);
                            }
                        }
                    }
                    
                    // Устанавливаем фото в модель
                    if (currentPhotos.Count > 0)
                    {
                        car.PhotoPath = currentPhotos[0]; // Первое фото как основное
                        
                        if (currentPhotos.Count > 1)
                        {
                            car.AdditionalPhotos = System.Text.Json.JsonSerializer.Serialize(currentPhotos.Skip(1).ToList());
                        }
                        else
                        {
                            car.AdditionalPhotos = null;
                        }
                    }
                    else
                    {
                        // Если фото нет, очищаем поля
                        car.PhotoPath = null;
                        car.AdditionalPhotos = null;
                    }
                    
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.CarId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505")
                {
                    var pgEx = (Npgsql.PostgresException)ex.InnerException;
                    if (pgEx.ConstraintName == "IX_Cars_LicensePlate")
                        ModelState.AddModelError("LicensePlate", "Автомобиль с таким госномером уже существует");
                    else if (pgEx.ConstraintName == "IX_Cars_Vin")
                        ModelState.AddModelError("Vin", "Автомобиль с таким VIN уже существует");
                    ViewBag.ClientId = new SelectList(_context.Clients, "ClientId", "FullName", car.ClientId);
                    return View(car);
                }
                return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.ClientId = new SelectList(_context.Clients, "ClientId", "FullName", car.ClientId);
            return View(car);
        }

        /// <summary>
        /// Сохраняет загруженное изображение автомобиля на сервере.
        /// </summary>
        /// <param name="photoFile">Файл изображения, загруженный через форму.</param>
        /// <returns>Относительный путь к сохраненному файлу.</returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если photoFile равен null или пуст.
        /// </exception>
        /// <exception cref="IOException">
        /// Выбрасывается при ошибках ввода-вывода при сохранении файла.
        /// </exception>
        /// <remarks>
        /// Создает уникальное имя файла на основе GUID и сохраняет его
        /// в папке wwwroot/uploads/cars/. Автоматически создает папку,
        /// если она не существует.
        /// </remarks>
        private async Task<string> SavePhotoAsync(IFormFile photoFile)
        {
            // Создаем уникальное имя файла
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars");
            
            // Создаем папку, если не существует
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            // Сохраняем файл
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(fileStream);
            }
            
            // Возвращаем относительный путь
            return $"/uploads/cars/{fileName}";
        }

        /// <summary>
        /// Удаляет файл фотографии автомобиля с сервера.
        /// </summary>
        /// <param name="photoPath">Относительный путь к файлу фотографии.</param>
        /// <remarks>
        /// Метод проверяет существование файла перед удалением.
        /// Не выбрасывает исключение, если файл не существует.
        /// </remarks>
        private void DeletePhoto(string photoPath)
        {
            if (!string.IsNullOrEmpty(photoPath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, photoPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления автомобиля.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля для удаления (nullable).</param>
        /// <returns>
        /// - Представление Delete с информацией об автомобиле, если найден.
        /// - NotFound (404), если автомобиль не найден или id равен null.
        /// </returns>
        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.CarId == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        /// <summary>
        /// Выполняет удаление автомобиля из базы данных.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля для удаления.</param>
        /// <returns>Перенаправление на страницу Index.</returns>
        /// <remarks>
        /// Метод также удаляет связанную фотографию автомобиля с сервера,
        /// если она существует.
        /// </remarks>
        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                // Удаляем основное фото
                if (!string.IsNullOrEmpty(car.PhotoPath))
                {
                    DeletePhoto(car.PhotoPath);
                }
                
                // Удаляем дополнительные фото
                if (!string.IsNullOrEmpty(car.AdditionalPhotos))
                {
                    var additionalPhotos = System.Text.Json.JsonSerializer.Deserialize<List<string>>(car.AdditionalPhotos);
                    if (additionalPhotos != null)
                    {
                        foreach (var photo in additionalPhotos)
                        {
                            DeletePhoto(photo);
                        }
                    }
                }
                
                _context.Cars.Remove(car);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование автомобиля по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор автомобиля для проверки.</param>
        /// <returns>true, если автомобиль существует; иначе false.</returns>
        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.CarId == id);
        }
    }
}