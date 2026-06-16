using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/orders")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CustomerOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMinioService _minio;

    public CustomerOrdersController(ApplicationDbContext db, IMinioService minio)
    {
        _db = db;
        _minio = minio;
    }

    private int GetClientId() => CustomerClientIdResolver.Resolve(User, _db);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAll()
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<List<OrderDto>>.Fail("Клиент не найден"));

        var orders = await _db.Orders
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .Include(o => o.Master)
            .Include(o => o.CompletedWorks!)
                .ThenInclude(cw => cw.Service)
            .Where(o =>
                (o.CarId.HasValue && o.Car != null && o.Car.ClientId == clientId) ||
                (o.TireId.HasValue && o.Tire != null && o.Tire.ClientId == clientId))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var dtos = await Task.WhenAll(orders.Select(ToDtoAsync));
        return Ok(ApiResponse<List<OrderDto>>.Ok(dtos.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id)
    {
        var clientId = GetClientId();
        var order = await _db.Orders
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .Include(o => o.Master)
            .Include(o => o.CompletedWorks!)
                .ThenInclude(cw => cw.Service)
            .FirstOrDefaultAsync(o => o.OrderNumber == id && (
                (o.CarId.HasValue && o.Car != null && o.Car.ClientId == clientId) ||
                (o.TireId.HasValue && o.Tire != null && o.Tire.ClientId == clientId)));

        if (order == null)
            return NotFound(ApiResponse<OrderDto>.Fail("Заказ не найден"));

        return Ok(ApiResponse<OrderDto>.Ok(await ToDtoAsync(order)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderRequest request)
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<OrderDto>.Fail("Клиент не найден"));

        if (request.CarId.HasValue)
        {
            var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == request.CarId && c.ClientId == clientId);
            if (car == null)
                return BadRequest(ApiResponse<OrderDto>.Fail("Автомобиль не найден"));
        }

        if (request.TireId.HasValue)
        {
            var tire = await _db.Tires.FirstOrDefaultAsync(t => t.TireId == request.TireId && t.ClientId == clientId);
            if (tire == null)
                return BadRequest(ApiResponse<OrderDto>.Fail("Шина не найдена"));
        }

        if (!request.CarId.HasValue && !request.TireId.HasValue)
            return BadRequest(ApiResponse<OrderDto>.Fail("Укажите автомобиль или шину"));

        if (request.ScheduledAt.HasValue)
        {
            var slotStart = PermTime.FromUtc(request.ScheduledAt.Value);
            var slotEnd = slotStart.AddMinutes(30);

            var postCount = await _db.Posts.CountAsync(p => !p.IsLocked);
            if (postCount == 0) postCount = 1;

            var occupiedCount = await _db.Orders.CountAsync(o =>
                o.ScheduledAt.HasValue
                && o.ScheduledAt >= slotStart
                && o.ScheduledAt < slotEnd
                && o.Status != "Оплачено");

            if (occupiedCount >= postCount)
                return Conflict(ApiResponse<OrderDto>.Fail("Это время уже занято"));
        }

        var order = new Order
        {
            CarId = request.CarId,
            TireId = request.TireId,
            OrderDate = PermTime.Now,
            ScheduledAt = request.ScheduledAt.HasValue
                ? PermTime.FromUtc(request.ScheduledAt.Value)
                : null
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        if (request.ServiceCodes?.Count > 0)
        {
            foreach (var sc in request.ServiceCodes)
            {
                _db.CompletedWorks.Add(new CompletedWork
                {
                    OrderNumber = order.OrderNumber,
                    ServiceCode = sc,
                    WheelCount = request.WheelCount,
                    CompletionTimeMin = 0,
                    WorkTotal = 0
                });
            }
            await _db.SaveChangesAsync();
        }

        if (request.HasOther || request.ServiceCodes == null || request.ServiceCodes.Count == 0)
        {
            var consultation = await _db.Services.FirstOrDefaultAsync(s => s.IsConsultation)
                ?? new Service
                {
                    ServiceName = "Консультация",
                    ServiceCost = 0,
                    IsConsultation = true
                };

            if (consultation.ServiceCode == 0)
                _db.Services.Add(consultation);

            await _db.SaveChangesAsync();

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

        var created = await _db.Orders
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .Include(o => o.Master)
            .Include(o => o.CompletedWorks!)
                .ThenInclude(cw => cw.Service)
            .FirstAsync(o => o.OrderNumber == order.OrderNumber);

        return CreatedAtAction(nameof(GetById), new { id = order.OrderNumber },
            ApiResponse<OrderDto>.Ok(await ToDtoAsync(created)));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Update(int id, [FromBody] EditOrderRequest request)
    {
        var clientId = GetClientId();
        var order = await _db.Orders
            .Include(o => o.CompletedWorks)
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .FirstOrDefaultAsync(o => o.OrderNumber == id && (
                (o.CarId.HasValue && o.Car != null && o.Car.ClientId == clientId) ||
                (o.TireId.HasValue && o.Tire != null && o.Tire.ClientId == clientId)));

        if (order == null)
            return NotFound(ApiResponse<OrderDto>.Fail("Заказ не найден"));

        if (order.Status != "Новый")
            return BadRequest(ApiResponse<OrderDto>.Fail("Редактирование возможно только для новых заказов"));

        order.ScheduledAt = request.ScheduledAt.HasValue
            ? PermTime.FromUtc(request.ScheduledAt.Value)
            : null;

        if (order.CompletedWorks?.Count > 0)
        {
            _db.CompletedWorks.RemoveRange(order.CompletedWorks);
            await _db.SaveChangesAsync();
        }

        if (request.ServiceCodes?.Count > 0)
        {
            foreach (var sc in request.ServiceCodes)
            {
                _db.CompletedWorks.Add(new CompletedWork
                {
                    OrderNumber = order.OrderNumber,
                    ServiceCode = sc,
                    WheelCount = request.WheelCount,
                    CompletionTimeMin = 0,
                    WorkTotal = 0
                });
            }
            await _db.SaveChangesAsync();
        }

        if (request.HasOther || request.ServiceCodes == null || request.ServiceCodes.Count == 0)
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

        var updated = await _db.Orders
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .Include(o => o.Master)
            .Include(o => o.CompletedWorks!)
                .ThenInclude(cw => cw.Service)
            .FirstAsync(o => o.OrderNumber == order.OrderNumber);

        return Ok(ApiResponse<OrderDto>.Ok(await ToDtoAsync(updated)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(int id)
    {
        var clientId = GetClientId();
        var order = await _db.Orders
            .Include(o => o.CompletedWorks)
            .Include(o => o.Car)
            .Include(o => o.Tire)
            .FirstOrDefaultAsync(o => o.OrderNumber == id && (
                (o.CarId.HasValue && o.Car != null && o.Car.ClientId == clientId) ||
                (o.TireId.HasValue && o.Tire != null && o.Tire.ClientId == clientId)));

        if (order == null)
            return NotFound(ApiResponse<object>.Fail("Заказ не найден"));

        if (order.Status != "Новый")
            return BadRequest(ApiResponse<object>.Fail("Нельзя отменить заказ, который уже обрабатывается"));

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }));
    }

    private async Task<OrderDto> ToDtoAsync(Order order)
    {
        var photoUrl = !string.IsNullOrEmpty(order.Car?.PhotoPath)
            ? await _minio.GetFileUrlAsync(order.Car.PhotoPath)
            : null;

        var additionalPhotosJson = order.Car?.AdditionalPhotos;
        if (!string.IsNullOrEmpty(additionalPhotosJson))
        {
            var keys = DeserializePhotoKeys(additionalPhotosJson);
            var urls = await Task.WhenAll(keys.Select(k => _minio.GetFileUrlAsync(k)));
            additionalPhotosJson = JsonSerializer.Serialize(urls.ToList());
        }

        return new OrderDto(
            order.OrderNumber,
            order.OrderDate,
            order.ScheduledAt?.ToString("O"),
            order.Status,
            order.PaymentDate.HasValue ? "Оплачено" : "Не оплачено",
            order.ClientTotal,
            order.Car != null
                ? new CarDto(
                    order.Car.CarId,
                    order.Car.Brand,
                    order.Car.Model,
                    order.Car.ManufactureYear,
                    order.Car.LicensePlate,
                    order.Car.Vin,
                    photoUrl,
                    additionalPhotosJson,
                    order.Car.FullInfo
                )
                : null,
            order.Master?.FullName,
            order.CompletedWorks?
                .Where(cw => cw.Service != null)
                .Select(cw => new CompletedWorkDto(
                    cw.WorkId,
                    cw.ServiceCode,
                    cw.Service!.ServiceName,
                    cw.WorkTotal,
                    cw.WheelCount
                ))
                .ToList() ?? [],
            order.Tire?.FullInfo
        );
    }

    private static List<string> DeserializePhotoKeys(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
