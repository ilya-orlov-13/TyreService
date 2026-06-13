using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Controllers.Api
{
    /// <summary>
    /// API контроллер для управления шинами.
    /// Обеспечивает RESTful API для CRUD операций с шинами.
    /// </summary>
    [Route("api/tires")]
    [ApiController]
    public class TiresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TiresController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить список всех шин
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTires([FromQuery] int? carId)
        {
            var query = _context.Tires
                .Include(t => t.Car)
                    .ThenInclude(c => c.Client)
                .AsQueryable();

            if (carId.HasValue)
                query = query.Where(t => t.CarId == carId.Value);

            var tires = await query
                .Select(t => new
                {
                    tireId = t.TireId,
                    carId = t.CarId,
                    tireType = t.TireType,
                    seasonality = t.Seasonality,
                    manufacturer = t.Manufacturer,
                    tireModel = t.TireModel,
                    size = t.Size,
                    loadIndex = t.LoadIndex,
                    wearPercentage = t.WearPercentage,
                    pressure = t.Pressure,
                    car = new
                    {
                        carId = t.Car.CarId,
                        brand = t.Car.Brand,
                        model = t.Car.Model,
                        licensePlate = t.Car.LicensePlate,
                        client = new
                        {
                            clientId = t.Car.Client.ClientId,
                            fullName = t.Car.Client.FullName,
                            phone = t.Car.Client.Phone
                        }
                    }
                })
                .ToListAsync();

            return Ok(new { success = true, data = tires });
        }

        /// <summary>
        /// Получить информацию о конкретной шине
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTire(int id)
        {
            var tire = await _context.Tires
                .Include(t => t.Car)
                    .ThenInclude(c => c.Client)
                .Where(t => t.TireId == id)
                .Select(t => new
                {
                    tireId = t.TireId,
                    carId = t.CarId,
                    tireType = t.TireType,
                    seasonality = t.Seasonality,
                    manufacturer = t.Manufacturer,
                    tireModel = t.TireModel,
                    size = t.Size,
                    loadIndex = t.LoadIndex,
                    wearPercentage = t.WearPercentage,
                    pressure = t.Pressure,
                    car = new
                    {
                        carId = t.Car.CarId,
                        brand = t.Car.Brand,
                        model = t.Car.Model,
                        licensePlate = t.Car.LicensePlate,
                        vin = t.Car.Vin,
                        client = new
                        {
                            clientId = t.Car.Client.ClientId,
                            fullName = t.Car.Client.FullName,
                            phone = t.Car.Client.Phone,
                            email = t.Car.Client.Email
                        }
                    }
                })
                .FirstOrDefaultAsync();

            if (tire == null)
                return NotFound(new { success = false, error = "Шина не найдена" });

            return Ok(new { success = true, data = tire });
        }

        /// <summary>
        /// Создать новую шину
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTire([FromBody] CreateTireRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Некорректные данные", errors = ModelState });

            // Проверка существования автомобиля
            var carExists = await _context.Cars.AnyAsync(c => c.CarId == request.CarId);
            if (!carExists)
                return BadRequest(new { success = false, error = "Автомобиль не найден" });

            var tire = new Tire
            {
                CarId = request.CarId,
                TireType = request.TireType,
                Seasonality = request.Seasonality,
                Manufacturer = request.Manufacturer ?? string.Empty,
                TireModel = request.TireModel ?? string.Empty,
                Size = request.Size ?? string.Empty,
                LoadIndex = request.LoadIndex ?? 0,
                WearPercentage = request.WearPercentage ?? 0,
                Pressure = request.Pressure ?? 0
            };

            _context.Tires.Add(tire);
            await _context.SaveChangesAsync();

            var createdTire = await _context.Tires
                .Include(t => t.Car)
                    .ThenInclude(c => c.Client)
                .Where(t => t.TireId == tire.TireId)
                .Select(t => new
                {
                    tireId = t.TireId,
                    carId = t.CarId,
                    tireType = t.TireType,
                    seasonality = t.Seasonality,
                    manufacturer = t.Manufacturer,
                    tireModel = t.TireModel,
                    size = t.Size,
                    loadIndex = t.LoadIndex,
                    wearPercentage = t.WearPercentage,
                    pressure = t.Pressure,
                    car = new
                    {
                        carId = t.Car.CarId,
                        brand = t.Car.Brand,
                        model = t.Car.Model,
                        licensePlate = t.Car.LicensePlate,
                        client = new
                        {
                            clientId = t.Car.Client.ClientId,
                            fullName = t.Car.Client.FullName
                        }
                    }
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetTire), new { id = tire.TireId }, new { success = true, data = createdTire });
        }

        /// <summary>
        /// Обновить информацию о шине
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTire(int id, [FromBody] UpdateTireRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Некорректные данные", errors = ModelState });

            var tire = await _context.Tires.FindAsync(id);
            if (tire == null)
                return NotFound(new { success = false, error = "Шина не найдена" });

            tire.CarId = request.CarId;
            tire.TireType = request.TireType;
            tire.Seasonality = request.Seasonality;
            tire.Manufacturer = request.Manufacturer ?? string.Empty;
            tire.TireModel = request.TireModel ?? string.Empty;
            tire.Size = request.Size ?? string.Empty;
            tire.LoadIndex = request.LoadIndex ?? 0;
            tire.WearPercentage = request.WearPercentage ?? 0;
            tire.Pressure = request.Pressure ?? 0;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Шина успешно обновлена" });
        }

        /// <summary>
        /// Удалить шину
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTire(int id)
        {
            var tire = await _context.Tires.FindAsync(id);
            if (tire == null)
                return NotFound(new { success = false, error = "Шина не найдена" });

            _context.Tires.Remove(tire);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Шина успешно удалена" });
        }

        /// <summary>
        /// Получить статистику по шинам
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetTireStats()
        {
            var totalTires = await _context.Tires.CountAsync();
            var tiresByType = await _context.Tires
                .GroupBy(t => t.TireType)
                .Select(g => new { tireType = g.Key, count = g.Count() })
                .ToListAsync();

            var tiresBySeasonality = await _context.Tires
                .GroupBy(t => t.Seasonality)
                .Select(g => new { seasonality = g.Key, count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalTires,
                    byType = tiresByType,
                    bySeasonality = tiresBySeasonality
                }
            });
        }
    }
}
