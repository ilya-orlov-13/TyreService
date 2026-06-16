using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICalculationService _calc;

        public OrdersController(ApplicationDbContext context, ICalculationService calc)
        {
            _context = context;
            _calc = calc;
        }

        /// <summary>
        /// Получить список всех заказов
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? status, [FromQuery] int? carId, [FromQuery] int? masterId)
        {
            var query = _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            if (carId.HasValue)
                query = query.Where(o => o.CarId == carId.Value);

            if (masterId.HasValue)
                query = query.Where(o => o.MasterId == masterId.Value);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    orderNumber = o.OrderNumber,
                    orderDate = o.OrderDate,
                    scheduledAt = o.ScheduledAt,
                    status = o.Status,
                    paymentDate = o.PaymentDate,
                    clientTotal = o.ClientTotal,
                    laborCost = o.LaborCost,
                    totalWorkMinutes = o.TotalWorkMinutes,
                    car = o.Car != null ? new
                    {
                        carId = o.Car.CarId,
                        brand = o.Car.Brand,
                        model = o.Car.Model,
                        licensePlate = o.Car.LicensePlate,
                        client = new
                        {
                            clientId = o.Car.Client.ClientId,
                            fullName = o.Car.Client.FullName,
                            phone = o.Car.Client.Phone
                        }
                    } : null,
                    master = o.Master != null ? new
                    {
                        masterId = o.Master.MasterId,
                        fullName = o.Master.FullName
                    } : null,
                    completedWorks = o.CompletedWorks.Select(cw => new
                    {
                        workId = cw.WorkId,
                        serviceCode = cw.ServiceCode,
                        serviceName = cw.Service.ServiceName,
                        wheelCount = cw.WheelCount,
                        completionTimeMin = cw.CompletionTimeMin,
                        workTotal = cw.WorkTotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = orders });
        }

        /// <summary>
        /// Получить информацию о конкретном заказе
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .Include(o => o.OrderConsumables)
                    .ThenInclude(oc => oc.Consumable)
                .Include(o => o.OrderComplexities)
                    .ThenInclude(oc => oc.ComplexityCoefficient)
                .Where(o => o.OrderNumber == id)
                .Select(o => new
                {
                    orderNumber = o.OrderNumber,
                    orderDate = o.OrderDate,
                    scheduledAt = o.ScheduledAt,
                    status = o.Status,
                    paymentDate = o.PaymentDate,
                    clientTotal = o.ClientTotal,
                    laborCost = o.LaborCost,
                    consumablesCost = o.ConsumablesCost,
                    discountPercent = o.DiscountPercent,
                    discountType = o.DiscountType,
                    totalWorkMinutes = o.TotalWorkMinutes,
                    workStartTime = o.WorkStartTime,
                    car = o.Car != null ? new
                    {
                        carId = o.Car.CarId,
                        brand = o.Car.Brand,
                        model = o.Car.Model,
                        licensePlate = o.Car.LicensePlate,
                        vin = o.Car.Vin,
                        photoPath = o.Car.PhotoPath,
                        client = new
                        {
                            clientId = o.Car.Client.ClientId,
                            fullName = o.Car.Client.FullName,
                            phone = o.Car.Client.Phone,
                            email = o.Car.Client.Email
                        }
                    } : null,
                    master = o.Master != null ? new
                    {
                        masterId = o.Master.MasterId,
                        fullName = o.Master.FullName,
                        hourlyRate = o.Master.HourlyRate
                    } : null,
                    completedWorks = o.CompletedWorks.Select(cw => new
                    {
                        workId = cw.WorkId,
                        serviceCode = cw.ServiceCode,
                        serviceName = cw.Service.ServiceName,
                        wheelCount = cw.WheelCount,
                        completionTimeMin = cw.CompletionTimeMin,
                        workTotal = cw.WorkTotal,
                        startedAt = cw.StartedAt
                    }).ToList(),
                    consumables = o.OrderConsumables.Select(oc => new
                    {
                        consumableId = oc.ConsumableId,
                        name = oc.Consumable.Name,
                        quantity = oc.Quantity,
                        sellPrice = oc.Consumable.SellPrice
                    }).ToList(),
                    complexities = o.OrderComplexities.Select(oc => new
                    {
                        complexityCoefficientId = oc.ComplexityCoefficientId,
                        name = oc.ComplexityCoefficient.Name,
                        factor = oc.ComplexityCoefficient.Factor
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            return Ok(new { success = true, data = order });
        }

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderApiRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Некорректные данные", errors = ModelState });

            // Проверка существования автомобиля или шины
            if (request.CarId.HasValue)
            {
                var carExists = await _context.Cars.AnyAsync(c => c.CarId == request.CarId.Value);
                if (!carExists)
                    return BadRequest(new { success = false, error = "Автомобиль не найден" });
            }
            
            if (request.TireId.HasValue)
            {
                var tireExists = await _context.Tires.AnyAsync(t => t.TireId == request.TireId.Value);
                if (!tireExists)
                    return BadRequest(new { success = false, error = "Шина не найдена" });
            }

            // Проверка существования мастера (если указан)
            if (request.MasterId.HasValue)
            {
                var masterExists = await _context.Masters.AnyAsync(m => m.MasterId == request.MasterId.Value);
                if (!masterExists)
                    return BadRequest(new { success = false, error = "Мастер не найден" });
            }

            var order = new Order
            {
                OrderDate = request.OrderDate ?? PermTime.Now,
                CarId = request.CarId,
                TireId = request.TireId,
                MasterId = request.MasterId,
                ScheduledAt = request.ScheduledAt.HasValue
                    ? PermTime.FromUtc(request.ScheduledAt.Value)
                    : null,
                Status = "Новый",
                DiscountPercent = request.DiscountPercent,
                DiscountType = request.DiscountType
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Добавляем коэффициенты сложности
            if (request.ComplexityCoefficientIds != null && request.ComplexityCoefficientIds.Any())
            {
                foreach (var ccId in request.ComplexityCoefficientIds)
                {
                    _context.OrderComplexities.Add(new OrderComplexity
                    {
                        OrderNumber = order.OrderNumber,
                        ComplexityCoefficientId = ccId
                    });
                }
            }

            // Добавляем расходники
            if (request.Consumables != null && request.Consumables.Any())
            {
                foreach (var cons in request.Consumables)
                {
                    var consumable = await _context.Consumables.FindAsync(cons.ConsumableId);
                    if (consumable != null)
                    {
                        _context.OrderConsumables.Add(new OrderConsumable
                        {
                            OrderNumber = order.OrderNumber,
                            ConsumableId = cons.ConsumableId,
                            Quantity = cons.Quantity
                        });
                    }
                }
            }

            // Добавляем услуги
            if (request.Services != null && request.Services.Any())
            {
                foreach (var svc in request.Services)
                {
                    _context.CompletedWorks.Add(new CompletedWork
                    {
                        OrderNumber = order.OrderNumber,
                        ServiceCode = svc.ServiceCode,
                        WheelCount = svc.WheelCount,
                        CompletionTimeMin = 0,
                        WorkTotal = 0
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Пересчитываем итоги заказа
            await _calc.CalculateOrderTotal(order.OrderNumber);

            var createdOrder = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Where(o => o.OrderNumber == order.OrderNumber)
                .Select(o => new
                {
                    orderNumber = o.OrderNumber,
                    orderDate = o.OrderDate,
                    status = o.Status,
                    scheduledAt = o.ScheduledAt,
                    car = o.Car != null ? new
                    {
                        carId = o.Car.CarId,
                        brand = o.Car.Brand,
                        model = o.Car.Model,
                        licensePlate = o.Car.LicensePlate,
                        client = new
                        {
                            clientId = o.Car.Client.ClientId,
                            fullName = o.Car.Client.FullName
                        }
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderNumber }, new { success = true, data = createdOrder });
        }

        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] OrderPreviewRequest request)
        {
            var items = new List<PreviewItemResult>();

            foreach (var reqItem in request.Items ?? new List<PreviewItemRequest>())
            {
                var basePrice = await _calc.GetDefaultTariff(reqItem.ServiceCode, request.CarClassId);
                var complexitySum = await GetComplexitySum(request.ComplexityCoefficientIds);
                var workTotal = basePrice * complexitySum * reqItem.WheelCount;
                var masterPercent = await GetMasterPercent(reqItem.ServiceCode, request.CarClassId);
                var masterShare = basePrice * masterPercent / 100m * complexitySum * reqItem.WheelCount;

                items.Add(new PreviewItemResult
                {
                    ServiceCode = reqItem.ServiceCode,
                    WheelCount = reqItem.WheelCount,
                    BasePrice = basePrice,
                    ComplexitySum = complexitySum,
                    WorkTotal = Math.Round(workTotal, 2),
                    MasterShare = Math.Round(masterShare, 2)
                });
            }

            var clientTotal = items.Sum(i => i.WorkTotal);
            var masterTotal = items.Sum(i => i.MasterShare);

            if (request.Consumables != null)
            {
                foreach (var rc in request.Consumables)
                {
                    var consumable = await _context.Consumables.FindAsync(rc.ConsumableId);
                    if (consumable != null)
                        clientTotal += consumable.SellPrice * rc.Quantity;
                }
            }

            if (request.DiscountPercent > 0)
            {
                var factor = 1m - request.DiscountPercent / 100m;
                clientTotal *= factor;
                if (request.DiscountType == "hard")
                    masterTotal *= factor;
            }

            var masterCount = request.MasterCount > 0 ? request.MasterCount : 1;

            return Ok(new OrderPreviewResponse
            {
                ClientTotal = Math.Round(clientTotal, 2),
                MasterShare = Math.Round(masterTotal, 2),
                MasterPerMaster = Math.Round(masterTotal / masterCount, 2),
                Items = items
            });
        }

        private async Task<decimal> GetComplexitySum(List<int>? ids)
        {
            if (ids == null || ids.Count == 0) return 1m;

            var factors = await _context.ComplexityCoefficients
                .Where(cc => ids.Contains(cc.ComplexityCoefficientId) && cc.IsActive)
                .Select(cc => cc.Factor)
                .ToListAsync();

            var sum = factors.Sum();
            return sum > 0 ? sum : 1m;
        }

        [HttpPost("{id}/assign-services")]
        public async Task<IActionResult> AssignServices(int id, [FromBody] AssignServicesRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .FirstOrDefaultAsync(o => o.OrderNumber == id);

            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            var unstartedWorks = order.CompletedWorks?
                .Where(cw => cw.CompletionTimeMin == 0)
                .ToList();

            if (unstartedWorks != null && unstartedWorks.Any())
            {
                _context.CompletedWorks.RemoveRange(unstartedWorks);
            }

            if (request.Services != null)
            {
                foreach (var s in request.Services)
                {
                    _context.CompletedWorks.Add(new CompletedWork
                    {
                        OrderNumber = id,
                        ServiceCode = s.ServiceCode,
                        WheelCount = s.WheelCount,
                        CompletionTimeMin = 0,
                        WorkTotal = 0
                    });
                }
            }

            await _context.SaveChangesAsync();
            await _calc.CalculateOrderTotal(id);

            return Ok(new { success = true });
        }

        [HttpPost("{id}/service/{workId}/start")]
        public async Task<IActionResult> StartServiceTimer(int id, int workId)
        {
            var cw = await _context.CompletedWorks
                .FirstOrDefaultAsync(w => w.WorkId == workId && w.OrderNumber == id);
            if (cw == null)
                return NotFound(new { success = false, error = "Работа не найдена" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null || !int.TryParse(masterIdClaim, out var masterId))
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var activeLog = await _context.WorkTimeLogs
                .AnyAsync(w => w.WorkId == workId && w.MasterId == masterId && w.EndTime == null);
            if (activeLog)
                return BadRequest(new { success = false, error = "Таймер уже запущен" });

            _context.WorkTimeLogs.Add(new WorkTimeLog
            {
                WorkId = workId,
                MasterId = masterId,
                StartTime = PermTime.Now,
                DurationMinutes = 0
            });

            cw.StartedAt = PermTime.Now;

            var order = await _context.Orders.FindAsync(id);
            if (order != null && order.Status == "Новый")
            {
                order.Status = "В работе";
                if (!order.WorkStartTime.HasValue)
                    order.WorkStartTime = PermTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, startedAt = cw.StartedAt.Value.ToString("o") });
        }

        [HttpPost("{id}/service/{workId}/stop")]
        public async Task<IActionResult> StopServiceTimer(int id, int workId)
        {
            var cw = await _context.CompletedWorks
                .Include(w => w.Service)
                .FirstOrDefaultAsync(w => w.WorkId == workId && w.OrderNumber == id);
            if (cw == null)
                return NotFound(new { success = false, error = "Работа не найдена" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null || !int.TryParse(masterIdClaim, out var masterId))
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var activeLog = await _context.WorkTimeLogs
                .FirstOrDefaultAsync(w => w.WorkId == workId && w.MasterId == masterId && w.EndTime == null);
            if (activeLog == null)
                return BadRequest(new { success = false, error = "Нет активного таймера" });

            var elapsed = (int)(PermTime.Now - activeLog.StartTime).TotalMinutes;
            if (elapsed < 1) elapsed = 1;
            activeLog.EndTime = PermTime.Now;
            activeLog.DurationMinutes = elapsed;

            if (cw.StartedAt.HasValue)
            {
                var cwElapsed = (int)(PermTime.Now - cw.StartedAt.Value).TotalMinutes;
                if (cwElapsed < 1) cwElapsed = 1;
                cw.CompletionTimeMin += cwElapsed;
                cw.StartedAt = null;

                if (cw.Service?.FixedDurationMin > 0)
                {
                    cw.TimeSavedMin = Math.Max(0, cw.Service.FixedDurationMin.Value - cw.CompletionTimeMin);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, timeSavedMin = cw.TimeSavedMin, durationMinutes = elapsed });
        }

        [HttpGet("{id}/service/{workId}/progress")]
        public async Task<IActionResult> GetServiceProgress(int id, int workId)
        {
            var cw = await _context.CompletedWorks
                .Include(w => w.Service)
                .FirstOrDefaultAsync(w => w.WorkId == workId && w.OrderNumber == id);
            if (cw == null)
                return NotFound(new { success = false, error = "Работа не найдена" });

            var logs = await _context.WorkTimeLogs
                .Include(w => w.Master)
                .Where(w => w.WorkId == workId)
                .ToListAsync();

            var totalMinutes = logs.Sum(w => w.DurationMinutes);
            var fixedDuration = cw.Service?.FixedDurationMin ?? 60;
            var progressPercent = fixedDuration > 0
                ? Math.Min(100, (int)((double)totalMinutes / fixedDuration * 100))
                : 0;

            var masters = logs
                .GroupBy(w => new { w.MasterId, w.Master.FullName })
                .Select(g => new
                {
                    masterId = g.Key.MasterId,
                    name = g.Key.FullName,
                    minutes = g.Sum(w => w.DurationMinutes)
                })
                .ToList();

            var masterIdClaim = User.FindFirstValue("MasterId");
            var isTimerActive = false;
            DateTime? runningSince = null;
            if (masterIdClaim != null && int.TryParse(masterIdClaim, out var currentMasterId))
            {
                var activeLog = logs.FirstOrDefault(w => w.MasterId == currentMasterId && w.EndTime == null);
                isTimerActive = activeLog != null;
                runningSince = activeLog?.StartTime;
            }

            return Ok(new
            {
                success = true,
                totalMinutes,
                fixedDurationMin = fixedDuration,
                progressPercent,
                isDone = cw.CompletionTimeMin > 0,
                isTimerActive,
                runningSince = runningSince?.ToString("o"),
                masters
            });
        }

        [HttpPost("{id}/service/{workId}/done")]
        public async Task<IActionResult> MarkServiceDone(int id, int workId)
        {
            var cw = await _context.CompletedWorks
                .Include(w => w.Service)
                .FirstOrDefaultAsync(w => w.WorkId == workId && w.OrderNumber == id);
            if (cw == null)
                return NotFound(new { success = false, error = "Работа не найдена" });

            var wasDone = cw.CompletionTimeMin > 0;
            if (wasDone)
            {
                cw.CompletionTimeMin = 0;
                cw.TimeSavedMin = 0;
            }
            else
            {
                cw.CompletionTimeMin = 1;
                cw.StartedAt = null;

                if (cw.Service?.FixedDurationMin > 0)
                {
                    cw.TimeSavedMin = Math.Max(0, cw.Service.FixedDurationMin.Value - cw.CompletionTimeMin);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, done = !wasDone, timeSavedMin = cw.TimeSavedMin });
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime? date)
        {
            var targetDate = date?.Date ?? PermTime.Today;
            var earlyMinutes = await _calc.GetAvailableEarlyMinutes(targetDate);

            var slots = new List<object>();
            var workStart = targetDate.AddHours(9);
            var workEnd = targetDate.AddHours(19);

            var ordersOnDate = await _context.Orders
                .Where(o => o.ScheduledAt.HasValue
                    && o.ScheduledAt.Value.Date == targetDate
                    && o.Status != "Оплачено")
                .OrderBy(o => o.ScheduledAt)
                .ToListAsync();

            var occupied = ordersOnDate
                .Select(o => o.ScheduledAt!.Value)
                .ToHashSet();

            for (var time = workStart; time < workEnd; time = time.AddMinutes(30))
            {
                var slotEnd = time.AddMinutes(30);
                var isFree = !occupied.Any(o => o >= time && o < slotEnd);

                var adjustedTime = time;
                if (earlyMinutes > 0 && isFree)
                {
                    adjustedTime = time.AddMinutes(-Math.Min(earlyMinutes, 30));
                    if (adjustedTime < workStart)
                        adjustedTime = workStart;
                }

                slots.Add(new
                {
                    time = time.ToString("HH:mm"),
                    adjustedTime = adjustedTime.ToString("HH:mm"),
                    isFree,
                    earlyAvailable = earlyMinutes > 0 && isFree
                });
            }

            return Ok(new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                earlyMinutes,
                slots
            });
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            if (order.Status != "В работе" && order.Status != "Новый")
                return BadRequest(new { success = false, error = "Заказ должен быть в статусе 'В работе' или 'Новый'" });

            if (order.WorkStartTime.HasValue)
            {
                var elapsed = (int)(PermTime.Now - order.WorkStartTime.Value).TotalMinutes;
                if (elapsed < 1) elapsed = 1;
                order.TotalWorkMinutes += elapsed;
                order.WorkStartTime = null;
            }

            order.Status = "Готов";
            await _context.SaveChangesAsync();

            await _calc.CalculateOrderTotal(id);
            var masterIds = new List<int> { order.MasterId ?? 0 };
            await _calc.CalculateMasterPayout(id, masterIds);

            // Calculate speed bonuses for services finished faster than fixed duration
            var completedWorks = await _context.CompletedWorks
                .Include(cw => cw.Service)
                .Where(cw => cw.OrderNumber == id && cw.TimeSavedMin > 0 && cw.MasterId.HasValue)
                .ToListAsync();

            foreach (var cw in completedWorks)
            {
                var hourlyRate = await _context.Masters
                    .Where(m => m.MasterId == cw.MasterId)
                    .Select(m => m.HourlyRate)
                    .FirstOrDefaultAsync();

                var savedHours = cw.TimeSavedMin / 60m;
                var bonusAmount = Math.Round(savedHours * hourlyRate * 0.5m, 2);

                if (bonusAmount > 0)
                {
                    _context.SpeedBonuses.Add(new SpeedBonus
                    {
                        MasterId = cw.MasterId.Value,
                        OrderNumber = id,
                        WorkId = cw.WorkId,
                        TimeSavedMin = cw.TimeSavedMin,
                        BonusAmount = bonusAmount
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        private async Task<decimal> GetMasterPercent(int serviceCode, int? carClassId)
        {
            if (carClassId.HasValue)
            {
                var tariff = await _context.ServiceTariffs
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && st.CarClassId == carClassId.Value);
                if (tariff != null)
                    return tariff.MasterSharePercent;
            }
            return 40m;
        }
    }

    public class OrderPreviewRequest
    {
        public int? CarClassId { get; set; }
        public List<PreviewItemRequest> Items { get; set; } = new();
        public List<int>? ComplexityCoefficientIds { get; set; }
        public List<PreviewConsumableRequest>? Consumables { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? DiscountType { get; set; }
        public int MasterCount { get; set; } = 1;
    }

    public class PreviewItemRequest
    {
        public int ServiceCode { get; set; }
        public int WheelCount { get; set; }
    }

    public class PreviewConsumableRequest
    {
        public int ConsumableId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderPreviewResponse
    {
        public decimal ClientTotal { get; set; }
        public decimal MasterShare { get; set; }
        public decimal MasterPerMaster { get; set; }
        public List<PreviewItemResult> Items { get; set; } = new();
    }

    public class AssignServicesRequest
    {
        public List<AssignServiceItem> Services { get; set; } = new();
    }

    public class AssignServiceItem
    {
        public int ServiceCode { get; set; }
        public int WheelCount { get; set; }
    }

    public class PreviewItemResult
    {
        public int ServiceCode { get; set; }
        public int WheelCount { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ComplexitySum { get; set; }
        public decimal WorkTotal { get; set; }
        public decimal MasterShare { get; set; }
    }
}
