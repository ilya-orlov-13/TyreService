using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard");
            if (User.IsInRole("Owner"))
                return RedirectToAction("Dashboard", "Home", new { area = "Owner" });
        }
        return View();
    }

    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.ClientsCount = _context.Clients.Count();
        ViewBag.CarsCount = _context.Cars.Count();
        ViewBag.OrdersCount = _context.Orders.Count();
        ViewBag.ServicesCount = _context.Services.Count();
        ViewBag.MastersCount = _context.Masters.Count();
        ViewBag.TiresCount = _context.Tires.Count();
        ViewBag.PostsCount = _context.Posts.Count();

        var allOrders = _context.Orders.ToList();
        var ordersWithCompletedWorks = _context.CompletedWorks
            .Select(cw => cw.OrderNumber).Distinct().ToList();
        
        ViewBag.ActiveOrdersCount = allOrders
            .Where(o => !ordersWithCompletedWorks.Contains(o.OrderNumber)).Count();
        ViewBag.CompletedOrdersCount = ordersWithCompletedWorks.Count;
        ViewBag.TodayOrdersCount = _context.Orders
            .Where(o => o.OrderDate.Date == DateTime.Today).Count();
        ViewBag.UnpaidOrdersCount = _context.Orders
            .Where(o => o.PaymentDate == null).Count();
        ViewBag.OrdersWithMastersCount = _context.Orders
            .Where(o => o.MasterId != null).Count();

        var topClients = _context.Clients
            .Select(c => new {
                Client = c,
                OrderCount = _context.Cars
                    .Where(car => car.ClientId == c.ClientId)
                    .SelectMany(car => car.Orders).Count()
            })
            .OrderByDescending(x => x.OrderCount).Take(3).ToList();
        ViewBag.TopClients = topClients;

        ViewBag.RecentOrders = _context.Orders
            .Include(o => o.Car).ThenInclude(c => c.Client)
            .Include(o => o.Master)
            .OrderByDescending(o => o.OrderDate).Take(5).ToList();

        ViewBag.ActivePosts = await _context.Posts
            .Include(p => p.ActiveSessions!)
                .ThenInclude(s => s.Master)
            .OrderBy(p => p.PostId)
            .ToListAsync();

        ViewBag.RecentPayouts = await _context.CompletedJobsPayouts
            .Include(p => p.Order)
                .ThenInclude(o => o!.Car)
                    .ThenInclude(c => c!.Client)
            .Include(p => p.Master)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
