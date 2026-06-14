using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _context.AdminUsers
                .Include(a => a.StaffPosition)
                .OrderBy(a => a.Login)
                .ToListAsync();
            return View(admins);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.StaffPositionId = new SelectList(
                await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync(),
                "StaffPositionId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Login,Email,FullName,StaffPositionId")] AdminUser adminUser, string Password)
        {
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            {
                ModelState.AddModelError("Password", "Пароль должен быть не менее 4 символов");
            }

            if (await _context.AdminUsers.AnyAsync(u => u.Login == adminUser.Login))
            {
                ModelState.AddModelError("Login", "Администратор с таким логином уже существует");
            }

            ModelState.Remove(nameof(AdminUser.PasswordHash));

            if (ModelState.IsValid)
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                adminUser.CreatedAt = PermTime.Now;
                _context.Add(adminUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StaffPositionId = new SelectList(
                await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync(),
                "StaffPositionId", "Name", adminUser.StaffPositionId);
            return View(adminUser);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var adminUser = await _context.AdminUsers.FindAsync(id);
            if (adminUser == null) return NotFound();

            ViewBag.StaffPositionId = new SelectList(
                await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync(),
                "StaffPositionId", "Name", adminUser.StaffPositionId);
            return View(adminUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdminUserId,Login,Email,FullName,StaffPositionId")] AdminUser adminUser, string? Password)
        {
            if (id != adminUser.AdminUserId) return NotFound();

            if (await _context.AdminUsers.AnyAsync(u => u.Login == adminUser.Login && u.AdminUserId != id))
            {
                ModelState.AddModelError("Login", "Администратор с таким логином уже существует");
            }

            ModelState.Remove(nameof(AdminUser.PasswordHash));

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.AdminUsers.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Login = adminUser.Login;
                    existing.Email = adminUser.Email;
                    existing.FullName = adminUser.FullName;
                    existing.StaffPositionId = adminUser.StaffPositionId;

                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        if (Password.Length < 4)
                        {
                            ModelState.AddModelError("Password", "Пароль должен быть не менее 4 символов");
                            ViewBag.StaffPositionId = new SelectList(
                                await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync(),
                                "StaffPositionId", "Name", adminUser.StaffPositionId);
                            return View(adminUser);
                        }
                        existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                    }

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.AdminUsers.Any(e => e.AdminUserId == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StaffPositionId = new SelectList(
                await _context.StaffPositions.OrderBy(p => p.Name).ToListAsync(),
                "StaffPositionId", "Name", adminUser.StaffPositionId);
            return View(adminUser);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var adminUser = await _context.AdminUsers.FirstOrDefaultAsync(m => m.AdminUserId == id);
            if (adminUser == null) return NotFound();
            return View(adminUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminUser = await _context.AdminUsers.FindAsync(id);
            if (adminUser != null)
            {
                _context.AdminUsers.Remove(adminUser);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
