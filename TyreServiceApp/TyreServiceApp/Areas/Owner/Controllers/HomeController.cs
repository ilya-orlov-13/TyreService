using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

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

        public async Task<IActionResult> Dashboard(string period = "month", DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var today = PermTime.Today;
            var (periodStart, periodEndExclusive, periodKey, periodLabel) = ResolveDashboardPeriod(period, dateFrom, dateTo, today);

            ViewBag.SelectedPeriod = periodKey;
            ViewBag.PeriodStart = periodStart;
            ViewBag.PeriodEnd = periodEndExclusive.AddDays(-1);
            ViewBag.PeriodLabel = periodLabel;

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

            var periodOrders = allOrders
                .Where(o => o.OrderDate >= periodStart && o.OrderDate < periodEndExclusive)
                .ToList();

            ViewBag.ActiveOrdersCount = allOrders
                .Where(o => !ordersWithCheckedWorks.Contains(o.OrderNumber)).Count();
            ViewBag.CompletedOrdersCount = ordersWithCheckedWorks.Count;
            ViewBag.TodayOrdersCount = periodOrders.Count;
            ViewBag.UnpaidOrdersCount = _context.Orders
                .Where(o => o.PaymentDate == null).Count();
            ViewBag.OrdersWithMastersCount = _context.Orders
                .Where(o => o.MasterId != null).Count();

            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            ViewBag.MonthOrdersCount = allOrders.Count(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart);

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

            ViewBag.TodayOrdersCount = periodOrders.Count;
            ViewBag.TodayRevenue = periodOrders.Sum(o => o.ClientTotal ?? 0);

            var todayProfit = 0m;
            foreach (var o in periodOrders)
            {
                var row = await _calc.CalculateOwnerRevenue(o.OrderNumber);
                todayProfit += row.NetProfit;
            }
            ViewBag.TodayProfit = Math.Round(todayProfit, 2);

            var completedToday = periodOrders.Count(o => o.Status == "Готов" || o.Status == "Оплачено");
            ViewBag.CompletedToday = completedToday;

            var ordersWithTotal = periodOrders.Where(o => o.ClientTotal > 0).ToList();
            var avgCheck = ordersWithTotal.Any() ? ordersWithTotal.Average(o => o.ClientTotal ?? 0) : 0;
            ViewBag.AverageCheck = Math.Round(avgCheck, 2);

            return View();
        }

        private static (DateTime Start, DateTime EndExclusive, string Key, string Label) ResolveDashboardPeriod(
            string? period,
            DateTime? dateFrom,
            DateTime? dateTo,
            DateTime today)
        {
            var key = string.IsNullOrWhiteSpace(period) ? "month" : period.Trim().ToLowerInvariant();

            DateTime start;
            DateTime endExclusive;
            string label;

            switch (key)
            {
                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    label = "Сегодня";
                    break;
                case "yesterday":
                    start = today.AddDays(-1);
                    endExclusive = today;
                    label = "Вчера";
                    break;
                case "week":
                    var diff = (int)today.DayOfWeek;
                    start = today.AddDays(-diff);
                    endExclusive = start.AddDays(7);
                    label = "Эта неделя";
                    break;
                case "month":
                    start = new DateTime(today.Year, today.Month, 1);
                    endExclusive = start.AddMonths(1);
                    label = "Этот месяц";
                    break;
                case "quarter":
                    var quarterMonth = ((today.Month - 1) / 3) * 3 + 1;
                    start = new DateTime(today.Year, quarterMonth, 1);
                    endExclusive = start.AddMonths(3);
                    label = "Этот квартал";
                    break;
                case "year":
                    start = new DateTime(today.Year, 1, 1);
                    endExclusive = start.AddYears(1);
                    label = "Этот год";
                    break;
                case "custom":
                    start = (dateFrom ?? today).Date;
                    endExclusive = (dateTo ?? dateFrom ?? today).Date.AddDays(1);
                    if (endExclusive <= start)
                        endExclusive = start.AddDays(1);
                    label = $"{start:dd.MM.yyyy} — {endExclusive.AddDays(-1):dd.MM.yyyy}";
                    break;
                default:
                    key = "month";
                    start = new DateTime(today.Year, today.Month, 1);
                    endExclusive = start.AddMonths(1);
                    label = "Этот месяц";
                    break;
            }

            return (start, endExclusive, key, label);
        }

        [HttpGet]
        public async Task<IActionResult> Distribution(DateTime? date)
        {
            var targetDate = date?.Date ?? PermTime.Today;
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
