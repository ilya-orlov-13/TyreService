using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Areas.Worker.Models;

namespace TyreServiceApp.Areas.Worker.Controllers
{
    [Area("Worker")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Post", new { area = "Worker" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Введите логин и пароль";
                return View();
            }

            var user = await _db.MasterUsers
                .Include(u => u.Master)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Неверный логин или пароль";
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Master?.FullName ?? user.Login),
                new(ClaimTypes.Role, "Master"),
                new("MasterId", user.MasterId.ToString()),
                new("MasterUserId", user.MasterUserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            return RedirectToAction("Index", "Post", new { area = "Worker" });
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Positions = new SelectList(await _db.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, int positionId, int rank, string login, string password, string? email)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Заполните все поля";
                ViewBag.Positions = new SelectList(await _db.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
                return View();
            }

            if (rank < 1 || rank > 6)
            {
                ViewBag.Error = "Разряд должен быть от 1 до 6";
                ViewBag.Positions = new SelectList(await _db.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
                return View();
            }

            if (!await _db.Positions.AnyAsync(p => p.PositionId == positionId))
            {
                ViewBag.Error = "Выберите должность";
                ViewBag.Positions = new SelectList(await _db.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
                return View();
            }

            if (await _db.MasterUsers.AnyAsync(u => u.Login == login))
            {
                ViewBag.Error = "Этот логин уже занят";
                ViewBag.Positions = new SelectList(await _db.Positions.OrderBy(p => p.Name).ToListAsync(), "PositionId", "Name");
                return View();
            }

            var master = new Master
            {
                FullName = fullName,
                PositionId = positionId,
                Rank = rank,
                HourlyRate = 0m
            };
            _db.Masters.Add(master);
            await _db.SaveChangesAsync();

            _db.MasterUsers.Add(new MasterUser
            {
                Login = login,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MasterId = master.MasterId
            });
            await _db.SaveChangesAsync();

            TempData["Registered"] = "Учётная запись мастера создана. Теперь можно войти.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth", new { area = "Worker" });
        }
    }
}
