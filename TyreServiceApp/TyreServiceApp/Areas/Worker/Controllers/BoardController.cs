using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Worker.Controllers
{
    [Area("Worker")]
    [Authorize(Roles = "Master")]
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return Challenge();

            var masterId = int.Parse(masterIdClaim);

            var activeSession = await _context.PostActiveSessions
                .Include(s => s.Post)
                .Include(s => s.Master)
                .FirstOrDefaultAsync(s => s.MasterId == masterId && s.EndedAt == null);

            if (activeSession == null)
                return RedirectToAction("Index", "Post", new { area = "Worker" });

            ViewBag.PostName = activeSession.Post?.Name ?? "Пост";
            ViewBag.PostId = activeSession.PostId;
            ViewBag.PostIsLocked = activeSession.Post?.IsLocked ?? false;

            var mastersOnPost = await _context.PostActiveSessions
                .Where(s => s.PostId == activeSession.PostId && s.EndedAt == null)
                .Include(s => s.Master)
                .Select(s => s.Master!.FullName)
                .ToListAsync();
            ViewBag.PostMasters = mastersOnPost;

            var services = await _context.Services
                .Where(s => !s.IsConsultation)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();
            ViewBag.Services = services;

            var masterIdsOnPost = await _context.PostActiveSessions
                .Where(s => s.PostId == activeSession.PostId && s.EndedAt == null)
                .Select(s => s.MasterId)
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .Where(o => o.MasterId == null || masterIdsOnPost.Contains(o.MasterId.Value))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Work(int id)
        {
            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return Challenge();

            var masterId = int.Parse(masterIdClaim);

            var activeSession = await _context.PostActiveSessions
                .Include(s => s.Post)
                .FirstOrDefaultAsync(s => s.MasterId == masterId && s.EndedAt == null);
            if (activeSession == null)
                return RedirectToAction("Index", "Post", new { area = "Worker" });

            ViewBag.PostName = activeSession.Post?.Name ?? "Пост";

            var order = await _context.Orders
                .Include(o => o.Car).ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks).ThenInclude(cw => cw.Service)
                .FirstOrDefaultAsync(o => o.OrderNumber == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Новый")
            {
                if (!order.MasterId.HasValue)
                    order.MasterId = masterId;

                order.Status = "В работе";
                order.WorkStartTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return View(order);
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
                order.PaymentDate = DateTime.Now;
            else if (status != "Оплачено" && order.PaymentDate.HasValue)
                order.PaymentDate = null;

            // Auto-assign master if logged in as a master and order has no master
            var masterIdClaim = User.FindFirstValue("MasterId");
            if (!order.MasterId.HasValue && status != "Новый")
            {
                if (masterIdClaim != null)
                {
                    order.MasterId = int.Parse(masterIdClaim);
                }
                else
                {
                    return Json(new { success = false, error = "Сначала назначьте мастера" });
                }
            }

            var prevStatus = order.Status;
            order.Status = status;

            // Timer: start when entering "В работе", stop on any leave
            if (status == "В работе" && prevStatus != "В работе")
            {
                order.WorkStartTime = DateTime.Now;
            }
            else if (prevStatus == "В работе" && status != "В работе")
            {
                if (order.WorkStartTime.HasValue)
                {
                    var elapsed = (int)(DateTime.Now - order.WorkStartTime.Value).TotalMinutes;
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
