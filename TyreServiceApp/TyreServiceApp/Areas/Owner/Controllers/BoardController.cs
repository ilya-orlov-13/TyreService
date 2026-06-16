using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Car).ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks).ThenInclude(cw => cw.Service)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var allowed = new[] { "Новый", "В работе", "Готов", "Оплачено" };
            if (!allowed.Contains(status))
                return Json(new { success = false, error = "Недопустимый статус" });

            if (status == "Оплачено" && !order.MasterId.HasValue)
                return Json(new { success = false, error = "Нельзя оплатить заказ без назначенного мастера" });

            if (status == "Оплачено" && !order.PaymentDate.HasValue)
                order.PaymentDate = PermTime.Now;
            else if (status != "Оплачено" && order.PaymentDate.HasValue)
                order.PaymentDate = null;

            if (!order.MasterId.HasValue && status != "Новый")
            {
                var anyMaster = await _context.Masters.Select(m => (int?)m.MasterId).FirstOrDefaultAsync();
                if (anyMaster == null)
                    return Json(new { success = false, error = "Нет доступных мастеров" });
                order.MasterId = anyMaster.Value;
            }

            var prevStatus = order.Status;
            order.Status = status;

            if (status == "В работе" && prevStatus != "В работе")
            {
                order.WorkStartTime = PermTime.Now;
            }
            else if (prevStatus == "В работе" && status != "В работе")
            {
                if (order.WorkStartTime.HasValue)
                {
                    var elapsed = (int)(PermTime.Now - order.WorkStartTime.Value).TotalMinutes;
                    if (elapsed < 1) elapsed = 1;
                    order.TotalWorkMinutes += elapsed;

                    order.WorkStartTime = null;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
