using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class CompletedWorksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompletedWorksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var works = await _context.CompletedWorks
                .Include(cw => cw.Order).ThenInclude(o => o.Car)
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .Where(cw => cw.CompletionTimeMin > 0 && cw.Order!.Status == "Оплачено")
                .ToListAsync();
            return View(works);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var work = await _context.CompletedWorks
                .Include(cw => cw.Order).ThenInclude(o => o.Car).ThenInclude(c => c.Client)
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .FirstOrDefaultAsync(m => m.WorkId == id);
            if (work == null) return NotFound();

            return View(work);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.OrderNumber = new SelectList(await _context.Orders.OrderByDescending(o => o.OrderNumber).ToListAsync(), "OrderNumber", "OrderNumber");
            ViewBag.ServiceCode = new SelectList(await _context.Services.OrderBy(s => s.ServiceName).ToListAsync(), "ServiceCode", "ServiceName");
            ViewBag.MasterId = new SelectList(await _context.Masters.OrderBy(m => m.FullName).ToListAsync(), "MasterId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderNumber,ServiceCode,MasterId,WheelCount,CompletionTimeMin")] CompletedWork work)
        {
            if (ModelState.IsValid)
            {
                var service = await _context.Services.FindAsync(work.ServiceCode);
                if (service != null)
                    work.WorkTotal = service.ServiceCost * work.WheelCount;

                _context.Add(work);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OrderNumber = new SelectList(await _context.Orders.OrderByDescending(o => o.OrderNumber).ToListAsync(), "OrderNumber", "OrderNumber", work.OrderNumber);
            ViewBag.ServiceCode = new SelectList(await _context.Services.OrderBy(s => s.ServiceName).ToListAsync(), "ServiceCode", "ServiceName", work.ServiceCode);
            ViewBag.MasterId = new SelectList(await _context.Masters.OrderBy(m => m.FullName).ToListAsync(), "MasterId", "FullName", work.MasterId);
            return View(work);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var work = await _context.CompletedWorks.FindAsync(id);
            if (work == null) return NotFound();

            ViewBag.OrderNumber = new SelectList(await _context.Orders.OrderByDescending(o => o.OrderNumber).ToListAsync(), "OrderNumber", "OrderNumber", work.OrderNumber);
            ViewBag.ServiceCode = new SelectList(await _context.Services.OrderBy(s => s.ServiceName).ToListAsync(), "ServiceCode", "ServiceName", work.ServiceCode);
            ViewBag.MasterId = new SelectList(await _context.Masters.OrderBy(m => m.FullName).ToListAsync(), "MasterId", "FullName", work.MasterId);
            return View(work);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkId,OrderNumber,ServiceCode,MasterId,WheelCount,CompletionTimeMin,WorkTotal")] CompletedWork work)
        {
            if (id != work.WorkId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(work);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CompletedWorks.Any(e => e.WorkId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OrderNumber = new SelectList(await _context.Orders.OrderByDescending(o => o.OrderNumber).ToListAsync(), "OrderNumber", "OrderNumber", work.OrderNumber);
            ViewBag.ServiceCode = new SelectList(await _context.Services.OrderBy(s => s.ServiceName).ToListAsync(), "ServiceCode", "ServiceName", work.ServiceCode);
            ViewBag.MasterId = new SelectList(await _context.Masters.OrderBy(m => m.FullName).ToListAsync(), "MasterId", "FullName", work.MasterId);
            return View(work);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var work = await _context.CompletedWorks
                .Include(cw => cw.Order).ThenInclude(o => o.Car)
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .FirstOrDefaultAsync(m => m.WorkId == id);
            if (work == null) return NotFound();

            return View(work);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var work = await _context.CompletedWorks.FindAsync(id);
            if (work != null) _context.CompletedWorks.Remove(work);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetServiceCost(int serviceCode)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceCode == serviceCode);
            return Json(new { success = true, cost = service?.ServiceCost ?? 0 });
        }
    }
}
