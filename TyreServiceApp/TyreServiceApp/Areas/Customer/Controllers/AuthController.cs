using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Customer.Models;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AuthController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Cabinet", new { area = "Customer" });

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var phone = string.Concat(model.Phone?.Where(char.IsDigit) ?? []);
            var all = await _db.Set<CustomerUser>()
                .Include(u => u.Client)
                .ToListAsync();
            var user = all.FirstOrDefault(u => string.Concat(u.Phone?.Where(char.IsDigit) ?? []) == phone);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Pin, user.PinHash))
            {
                ModelState.AddModelError("", "Неверный телефон или PIN-код");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Client?.FullName ?? user.Phone),
                new("Phone", user.Phone),
                new("CustomerId", user.Id.ToString()),
                new("ClientId", user.ClientId?.ToString() ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var phone = string.Concat(model.Phone?.Where(char.IsDigit) ?? []);
            var all = await _db.Clients.ToListAsync();
            if (all.Any(c => string.Concat(c.Phone?.Where(char.IsDigit) ?? []) == phone))
            {
                ModelState.AddModelError("Phone", "Этот телефон уже зарегистрирован");
                return View(model);
            }

            var client = new Client
            {
                FullName = model.FullName,
                Phone = phone
            };
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            var user = new CustomerUser
            {
                Phone = phone,
                PinHash = BCrypt.Net.BCrypt.HashPassword(model.Pin),
                ClientId = client.ClientId
            };
            _db.Add(user);
            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, client.FullName),
                new("Phone", user.Phone),
                new("CustomerId", user.Id.ToString()),
                new("ClientId", client.ClientId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Cabinet", new { area = "Customer" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Cabinet", new { area = "Customer" });
        }
    }
}
