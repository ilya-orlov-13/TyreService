using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/slots")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class SlotsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICalculationService _calc;

    public SlotsController(ApplicationDbContext db, ICalculationService calc)
    {
        _db = db;
        _calc = calc;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TimeSlotDto>>>> GetAvailable([FromQuery] DateTime? date)
    {
        var targetDate = date?.Date ?? PermTime.Today;

        var earlyMinutes = await _calc.GetAvailableEarlyMinutes(targetDate);

        var postCount = await _db.Posts.CountAsync(p => !p.IsLocked);
        if (postCount == 0) postCount = 1;

        var ordersOnDate = await _db.Orders
            .Where(o => o.ScheduledAt.HasValue
                && o.ScheduledAt.Value.Date == targetDate
                && o.Status != "Оплачено")
            .Select(o => o.ScheduledAt!.Value)
            .ToListAsync();

        var workStart = targetDate.AddHours(9);
        var workEnd = targetDate.AddHours(19);
        var slots = new List<TimeSlotDto>();

        for (var time = workStart; time < workEnd; time = time.AddMinutes(30))
        {
            var slotEnd = time.AddMinutes(30);
            var occupiedCount = ordersOnDate.Count(o => o >= time && o < slotEnd);
            var isFree = occupiedCount < postCount;

            var adjustedTime = time;
            if (earlyMinutes > 0 && isFree)
            {
                adjustedTime = time.AddMinutes(-Math.Min(earlyMinutes, 30));
                if (adjustedTime < workStart)
                    adjustedTime = workStart;
            }

            slots.Add(new TimeSlotDto(adjustedTime, isFree));
        }

        return Ok(ApiResponse<List<TimeSlotDto>>.Ok(slots));
    }
}
