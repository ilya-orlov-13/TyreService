using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Services;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICalculationService _calc;

        public HomeController(ApplicationDbContext context, ICalculationService calc)
        {
            _context = context;
            _calc = calc;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.ClientsCount = _context.Clients.Count();
            ViewBag.CarsCount = _context.Cars.Count();
            ViewBag.OrdersCount = _context.Orders.Count();
            ViewBag.ServicesCount = _context.Services.Count();
            ViewBag.MastersCount = _context.Masters.Count();
            ViewBag.TiresCount = _context.Tires.Count();
            ViewBag.PostsCount = _context.Posts.Count();
            var checkedWorks = _context.CompletedWorks.Where(cw => cw.CompletionTimeMin > 0);

            var allOrders = _context.Orders.ToList();
            var paidOrderNumbers = _context.Orders
                .Where(o => o.Status == "Оплачено")
                .Select(o => o.OrderNumber).ToList();
            var ordersWithCheckedWorks = checkedWorks
                .Select(cw => cw.OrderNumber).Distinct().ToList();

            ViewBag.ActiveOrdersCount = allOrders
                .Where(o => !ordersWithCheckedWorks.Contains(o.OrderNumber)).Count();
            ViewBag.CompletedOrdersCount = ordersWithCheckedWorks.Count;
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

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            ViewBag.DiscountStats = new
            {
                OrdersWithDiscount = _context.Orders.Count(o => o.DiscountPercent > 0),
                SoftDiscountOrders = _context.Orders.Count(o => o.DiscountPercent > 0 && o.DiscountType == "soft"),
                HardDiscountOrders = _context.Orders.Count(o => o.DiscountPercent > 0 && o.DiscountType == "hard"),
                SoftDiscountTotal = _context.Orders
                    .Where(o => o.DiscountPercent > 0 && o.DiscountType == "soft")
                    .Sum(o => o.ClientTotal ?? 0) * 0m,
                MonthSoftLoss = _context.Orders
                    .Where(o => o.DiscountPercent > 0 && o.DiscountType == "soft"
                        && o.OrderDate >= monthStart)
                    .Sum(o => (o.ClientTotal ?? 0) * o.DiscountPercent / 100m),
                MonthHardSavings = _context.Orders
                    .Where(o => o.DiscountPercent > 0 && o.DiscountType == "hard"
                        && o.OrderDate >= monthStart)
                    .Sum(o => (o.LaborCost ?? 0) * o.DiscountPercent / 100m),
            };

            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .ToListAsync();

            ViewBag.TodayOrdersCount = todayOrders.Count;
            ViewBag.TodayRevenue = todayOrders.Sum(o => o.ClientTotal ?? 0);

            var todayProfit = 0m;
            foreach (var o in todayOrders)
            {
                var row = await _calc.CalculateOwnerRevenue(o.OrderNumber);
                todayProfit += row.NetProfit;
            }
            ViewBag.TodayProfit = Math.Round(todayProfit, 2);

            var completedToday = todayOrders.Count(o => o.Status == "Готов" || o.Status == "Оплачено");
            ViewBag.CompletedToday = completedToday;

            var ordersWithTotal = todayOrders.Where(o => o.ClientTotal > 0).ToList();
            var avgCheck = ordersWithTotal.Any() ? ordersWithTotal.Average(o => o.ClientTotal ?? 0) : 0;
            ViewBag.AverageCheck = Math.Round(avgCheck, 2);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Distribution(DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today;
            var dist = await _calc.CalculateMonthlySpeedBonuses(targetDate.Year, targetDate.Month);

            ViewBag.TargetDate = targetDate;
            ViewBag.MonthlyBonuses = await _context.SpeedBonuses
                .Include(sb => sb.Master)
                .Where(sb => sb.CreatedAt.Year == targetDate.Year && sb.CreatedAt.Month == targetDate.Month)
                .ToListAsync();

            var service = new DistributionService(_context);
            var distributionResult = await service.DistributeOrders(targetDate);

            return View(distributionResult);
        }
    }
}
