using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Hubs;
using TyreServiceApp.Models;
using TyreServiceApp.Services;

namespace TyreServiceApp.Controllers.Api
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize(Roles = "Master")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ICalculationService _calc;

        public BookingsController(ApplicationDbContext context, IHubContext<OrderHub> hubContext, ICalculationService calc)
        {
            _context = context;
            _hubContext = hubContext;
            _calc = calc;
        }

        [HttpPatch("{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            if (order.Status != "Новый")
                return BadRequest(new { success = false, error = "Заказ должен быть в статусе 'Новый'" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.MasterId == masterId && s.EndedAt == null);
            if (!hasSession)
                return BadRequest(new { success = false, error = "У вас нет активной сессии на посту" });

            if (!order.MasterId.HasValue)
                order.MasterId = masterId;

            order.Status = "В работе";
            order.WorkStartTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.CompletedWorks)
                .FirstOrDefaultAsync(o => o.OrderNumber == id);

            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            if (order.Status != "В работе")
                return BadRequest(new { success = false, error = "Заказ должен быть в статусе 'В работе'" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var activeSession = await _context.PostActiveSessions
                .FirstOrDefaultAsync(s => s.MasterId == masterId && s.EndedAt == null);

            if (activeSession == null)
                return BadRequest(new { success = false, error = "У вас нет активной сессии на посту" });

            var postId = activeSession.PostId;

            var masterIdsOnPost = await _context.PostActiveSessions
                .Where(s => s.PostId == postId && s.EndedAt == null)
                .Select(s => s.MasterId)
                .ToListAsync();

            if (masterIdsOnPost.Count == 0)
                return BadRequest(new { success = false, error = "На посту нет мастеров" });

            await _calc.CalculateOrderTotal(id);

            var payouts = await _calc.CalculateMasterPayout(id, masterIdsOnPost);

            if (order.WorkStartTime.HasValue)
            {
                var elapsed = (int)(DateTime.Now - order.WorkStartTime.Value).TotalMinutes;
                if (elapsed < 1) elapsed = 1;
                order.TotalWorkMinutes += elapsed;
                order.WorkStartTime = null;
            }

            order.Status = "Готов";
            await _context.SaveChangesAsync();

            var totalPayout = payouts.Sum(p => p.Amount);

            try
            {
                await _hubContext.Clients.All.SendAsync("OrderCompleted", new
                {
                    orderNumber = order.OrderNumber,
                    totalPayout = totalPayout
                });
            }
            catch
            {
                // WebSocket notification is non-critical
            }

            return Ok(new
            {
                success = true,
                payouts = payouts.Select(p => new
                {
                    masterId = p.MasterId,
                    amount = p.Amount
                })
            });
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.WorkTimeLogs)
                .FirstOrDefaultAsync(o => o.OrderNumber == id);
            if (order == null)
                return NotFound(new { success = false, error = "Заказ не найден" });

            if (order.Status != "В работе")
                return BadRequest(new { success = false, error = "Заказ должен быть в статусе 'В работе'" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.MasterId == int.Parse(masterIdClaim) && s.EndedAt == null);
            if (!hasSession)
                return BadRequest(new { success = false, error = "У вас нет активной сессии на посту" });

            if (order.CompletedWorks != null)
            {
                foreach (var cw in order.CompletedWorks)
                {
                    if (cw.WorkTimeLogs?.Any() == true)
                        _context.WorkTimeLogs.RemoveRange(cw.WorkTimeLogs);
                    cw.StartedAt = null;
                    cw.CompletionTimeMin = 0;
                }
            }

            order.Status = "Новый";
            order.WorkStartTime = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

}
