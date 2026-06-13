using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Controllers.Api
{
    /// <summary>
    /// API контроллер для управления автомобилями.
    /// Обеспечивает RESTful API для CRUD операций с автомобилями.
    /// </summary>
    [Route("api/cars")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить список всех автомобилей
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCars()
        {
            var cars = await _context.Cars
                .Include(c => c.Client)
                .Select(c => new
                {
                    carId = c.CarId,
                    brand = c.Brand,
                    model = c.Model,
                    manufacturerYear = c.ManufactureYear,
                    licensePlate = c.LicensePlate,
                    vin = c.Vin,
                    photoPath = c.PhotoPath,
                    additionalPhotos = c.AdditionalPhotos,
                    client = new
                    {
                        clientId = c.Client.ClientId,
                        fullName = c.Client.FullName,
                        phone = c.Client.Phone,
                        email = c.Client.Email
                    }
                })
                .ToListAsync();

            return Ok(new { success = true, data = cars });
        }

        /// <summary>
        /// Получить информацию о конкретном автомобиле
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCar(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Client)
                .Where(c => c.CarId == id)
                .Select(c => new
                {
                    carId = c.CarId,
                    brand = c.Brand,
                    model = c.Model,
                    manufacturerYear = c.ManufactureYear,
                    licensePlate = c.LicensePlate,
                    vin = c.Vin,
                    photoPath = c.PhotoPath,
                    additionalPhotos = c.AdditionalPhotos,
                    client = new
                    {
                        clientId = c.Client.ClientId,
                        fullName = c.Client.FullName,
                        phone = c.Client.Phone,
                        email = c.Client.Email
                    }
                })
                .FirstOrDefaultAsync();

            if (car == null)
                return NotFound(new { success = false, error = "Автомобиль не найден" });

            return Ok(new { success = true, data = car });
        }

        /// <summary>
        /// Создать новый автомобиль
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCar([FromBody] CreateCarRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Некорректные данные", errors = ModelState });

            // Проверка существования клиента
            var clientExists = await _context.Clients.AnyAsync(c => c.ClientId == request.ClientId);
            if (!clientExists)
                return BadRequest(new { success = false, error = "Клиент не найден" });

            // Проверка уникальности госномера
            if (await _context.Cars.AnyAsync(c => c.LicensePlate == request.LicensePlate))
                return BadRequest(new { success = false, error = "Автомобиль с таким госномером уже существует" });

            // Проверка уникальности VIN
            if (!string.IsNullOrEmpty(request.Vin) && await _context.Cars.AnyAsync(c => c.Vin == request.Vin))
                return BadRequest(new { success = false, error = "Автомобиль с таким VIN уже существует" });

            var car = new Car
            {
                ClientId = request.ClientId,
                Brand = request.Brand,
                Model = request.Model,
                ManufactureYear = request.ManufactureYear,
                LicensePlate = request.LicensePlate,
                Vin = request.Vin,
                PhotoPath = request.PhotoPath,
                AdditionalPhotos = request.AdditionalPhotos
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            var createdCar = await _context.Cars
                .Include(c => c.Client)
                .Where(c => c.CarId == car.CarId)
                .Select(c => new
                {
                    carId = c.CarId,
                    brand = c.Brand,
                    model = c.Model,
                    manufacturerYear = c.ManufactureYear,
                    licensePlate = c.LicensePlate,
                    vin = c.Vin,
                    photoPath = c.PhotoPath,
                    additionalPhotos = c.AdditionalPhotos,
                    client = new
                    {
                        clientId = c.Client.ClientId,
                        fullName = c.Client.FullName,
                        phone = c.Client.Phone,
                        email = c.Client.Email
                    }
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetCar), new { id = car.CarId }, new { success = true, data = createdCar });
        }

        /// <summary>
        /// Обновить информацию об автомобиле
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] UpdateCarRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Некорректные данные", errors = ModelState });

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound(new { success = false, error = "Автомобиль не найден" });

            // Проверка уникальности госномера
            if (await _context.Cars.AnyAsync(c => c.LicensePlate == request.LicensePlate && c.CarId != id))
                return BadRequest(new { success = false, error = "Автомобиль с таким госномером уже существует" });

            // Проверка уникальности VIN
            if (!string.IsNullOrEmpty(request.Vin) && await _context.Cars.AnyAsync(c => c.Vin == request.Vin && c.CarId != id))
                return BadRequest(new { success = false, error = "Автомобиль с таким VIN уже существует" });

            car.ClientId = request.ClientId;
            car.Brand = request.Brand;
            car.Model = request.Model;
            car.ManufactureYear = request.ManufactureYear;
            car.LicensePlate = request.LicensePlate;
            car.Vin = request.Vin;
            car.PhotoPath = request.PhotoPath;
            car.AdditionalPhotos = request.AdditionalPhotos;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Автомобиль успешно обновлен" });
        }

        /// <summary>
        /// Удалить автомобиль
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound(new { success = false, error = "Автомобиль не найден" });

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Автомобиль успешно удален" });
        }

        /// <summary>
        /// Поиск автомобилей по госномеру
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCars([FromQuery] string? licensePlate, [FromQuery] int? clientId)
        {
            var query = _context.Cars
                .Include(c => c.Client)
                .AsQueryable();

            if (!string.IsNullOrEmpty(licensePlate))
                query = query.Where(c => c.LicensePlate.Contains(licensePlate));

            if (clientId.HasValue)
                query = query.Where(c => c.ClientId == clientId.Value);

            var cars = await query
                .Select(c => new
                {
                    carId = c.CarId,
                    brand = c.Brand,
                    model = c.Model,
                    manufacturerYear = c.ManufactureYear,
                    licensePlate = c.LicensePlate,
                    vin = c.Vin,
                    photoPath = c.PhotoPath,
                    client = new
                    {
                        clientId = c.Client.ClientId,
                        fullName = c.Client.FullName,
                        phone = c.Client.Phone
                    }
                })
                .ToListAsync();

            return Ok(new { success = true, data = cars });
        }
    }
}
