using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Worker.Models;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class MastersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MastersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var masters = await _context.Masters.Include(m => m.Position).OrderBy(m => m.FullName).ToListAsync();
            var masterIds = masters.Select(m => m.MasterId).ToList();

            var revenueData = await _context.CompletedWorks
                .Where(cw => cw.CompletionTimeMin > 0 && cw.MasterId != null && masterIds.Contains(cw.MasterId.Value))
                .GroupBy(cw => cw.MasterId)
                .Select(g => new { MasterId = g.Key, Total = g.Sum(cw => cw.WorkTotal) })
                .ToListAsync();

            var payoutData = await _context.CompletedJobsPayouts
                .Where(p => masterIds.Contains(p.MasterId))
                .GroupBy(p => p.MasterId)
                .Select(g => new { MasterId = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            ViewBag.Revenue = revenueData.ToDictionary(d => d.MasterId, d => d.Total);
            ViewBag.Payouts = payoutData.ToDictionary(d => d.MasterId, d => d.Total);

            return View(masters);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var master = await _context.Masters
                .Include(m => m.Position)
                .FirstOrDefaultAsync(m => m.MasterId == id);
            if (master == null) return NotFound();

            return View(master);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.PositionId = new SelectList(await _context.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,PositionId,Rank")] Master master,
            string? login, string? password)
        {
            if (ModelState.IsValid)
            {
                _context.Add(master);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
                {
                    var masterUser = new MasterUser
                    {
                        Login = login,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        MasterId = master.MasterId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(masterUser);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.PositionId = new SelectList(await _context.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name", master.PositionId);
            return View(master);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var master = await _context.Masters.FindAsync(id);
            if (master == null) return NotFound();

            var masterUser = await _context.MasterUsers.FirstOrDefaultAsync(u => u.MasterId == id);
            ViewBag.MasterLogin = masterUser?.Login ?? "";
            ViewBag.PositionId = new SelectList(await _context.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name", master.PositionId);
            return View(master);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MasterId,FullName,PositionId,Rank")] Master master,
            string? login, string? password)
        {
            if (id != master.MasterId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(master);
                    await _context.SaveChangesAsync();

                    var masterUser = await _context.MasterUsers.FirstOrDefaultAsync(u => u.MasterId == id);
                    if (masterUser != null)
                    {
                        if (!string.IsNullOrWhiteSpace(login))
                            masterUser.Login = login;
                        if (!string.IsNullOrWhiteSpace(password))
                            masterUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                        await _context.SaveChangesAsync();
                    }
                    else if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
                    {
                        masterUser = new MasterUser
                        {
                            Login = login,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                            MasterId = id,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Add(masterUser);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Masters.Any(e => e.MasterId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PositionId = new SelectList(await _context.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name", master.PositionId);
            return View(master);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var master = await _context.Masters.Include(m => m.Position).FirstOrDefaultAsync(m => m.MasterId == id);
            if (master == null) return NotFound();

            return View(master);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var master = await _context.Masters.FindAsync(id);
            if (master != null) _context.Masters.Remove(master);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
