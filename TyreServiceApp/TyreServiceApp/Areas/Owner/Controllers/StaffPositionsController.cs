using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class StaffPositionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffPositionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] StaffPosition staffPosition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(staffPosition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staffPosition);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var staffPosition = await _context.StaffPositions.FindAsync(id);
            if (staffPosition == null) return NotFound();
            return View(staffPosition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StaffPositionId,Name")] StaffPosition staffPosition)
        {
            if (id != staffPosition.StaffPositionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staffPosition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.StaffPositions.Any(e => e.StaffPositionId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staffPosition);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var staffPosition = await _context.StaffPositions.FirstOrDefaultAsync(m => m.StaffPositionId == id);
            if (staffPosition == null) return NotFound();

            return View(staffPosition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staffPosition = await _context.StaffPositions.FindAsync(id);
            if (staffPosition != null)
            {
                if (await _context.AdminUsers.AnyAsync(u => u.StaffPositionId == id))
                {
                    TempData["Error"] = "Нельзя удалить должность, к которой привязаны администраторы";
                    return RedirectToAction(nameof(Index));
                }
                _context.StaffPositions.Remove(staffPosition);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
