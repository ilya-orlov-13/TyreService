using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TyreServiceApp.Data;

namespace TyreServiceApp.Controllers
{
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
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            var defaultLogin = _config["DefaultCredentials:AdminLogin"];
            var defaultHash = _config["DefaultCredentials:AdminPasswordHash"];
            if (!string.IsNullOrEmpty(defaultLogin) && login == defaultLogin && BCrypt.Net.BCrypt.Verify(password, defaultHash))
            {
                return await SignInAdmin(login, login);
            }

            var adminUser = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Login == login);
            if (adminUser != null && BCrypt.Net.BCrypt.Verify(password, adminUser.PasswordHash))
            {
                return await SignInAdmin(adminUser.FullName ?? adminUser.Login, adminUser.Login);
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        private async Task<IActionResult> SignInAdmin(string name, string login)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Admin")
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

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
