using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Owner"))
                return RedirectToAction("Dashboard", "Home", new { area = "Owner" });
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

            var defaultLogin = _config["DefaultCredentials:OwnerLogin"];
            var defaultHash = _config["DefaultCredentials:OwnerPasswordHash"];
            if (!string.IsNullOrEmpty(defaultLogin) && login == defaultLogin && BCrypt.Net.BCrypt.Verify(password, defaultHash))
            {
                return await SignInOwner(login, login);
            }

            var ownerUser = await _db.OwnerUsers.FirstOrDefaultAsync(u => u.Login == login);
            if (ownerUser != null && BCrypt.Net.BCrypt.Verify(password, ownerUser.PasswordHash))
            {
                return await SignInOwner(ownerUser.FullName ?? ownerUser.Login, ownerUser.Login);
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        private async Task<IActionResult> SignInOwner(string name, string login)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Owner")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Dashboard", "Home", new { area = "Owner" });
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Owner"))
                return RedirectToAction("Dashboard", "Home", new { area = "Owner" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string login, string password, string confirmPassword, string? fullName, string? email)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Заполните логин и пароль";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Пароль должен быть не менее 6 символов";
                return View();
            }

            if (await _db.OwnerUsers.AnyAsync(u => u.Login == login))
            {
                ViewBag.Error = "Этот логин уже занят";
                return View();
            }

            if (!string.IsNullOrEmpty(email) && await _db.OwnerUsers.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Этот email уже используется";
                return View();
            }

            _db.OwnerUsers.Add(new OwnerUser
            {
                Login = login,
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Владелец зарегистрирован. Теперь можно войти.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
