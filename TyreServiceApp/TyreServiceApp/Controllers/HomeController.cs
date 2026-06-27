using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Utils;

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
    public async Task<IActionResult> Dashboard(string period = "month", DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var today = PermTime.Today;
        var (periodStart, periodEndExclusive, periodKey, periodLabel) = ResolveDashboardPeriod(period, dateFrom, dateTo, today);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        ViewBag.SelectedPeriod = periodKey;
        ViewBag.PeriodStart = periodStart;
        ViewBag.PeriodEnd = periodEndExclusive.AddDays(-1);
        ViewBag.PeriodLabel = periodLabel;

        ViewBag.ClientsCount = await _context.Clients.CountAsync();
        ViewBag.CarsCount = await _context.Cars.CountAsync();
        ViewBag.OrdersCount = await _context.Orders.CountAsync();
        ViewBag.ServicesCount = await _context.Services.CountAsync();
        ViewBag.MastersCount = await _context.Masters.CountAsync();
        ViewBag.TiresCount = await _context.Tires.CountAsync();

        var ordersWithCompletedWorks = await _context.CompletedWorks
            .AsNoTracking()
            .Select(cw => cw.OrderNumber)
            .Distinct()
            .ToListAsync();

        var completedOrderNumbers = ordersWithCompletedWorks.ToHashSet();

        var allOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.CompletedWorks)
            .Include(o => o.OrderConsumables!)
                .ThenInclude(oc => oc.Consumable)
            .Include(o => o.CompletedJobsPayouts)
            .ToListAsync();

        var periodOrders = allOrders
            .Where(o => o.OrderDate >= periodStart && o.OrderDate < periodEndExclusive)
            .ToList();

        ViewBag.ActiveOrdersCount = allOrders.Count(o => !completedOrderNumbers.Contains(o.OrderNumber));
        ViewBag.CompletedOrdersCount = ordersWithCompletedWorks.Count;
        ViewBag.TodayOrdersCount = periodOrders.Count;
        ViewBag.MonthOrdersCount = allOrders.Count(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart);
        ViewBag.UnpaidOrdersCount = allOrders.Count(o => o.PaymentDate == null);
        ViewBag.OrdersWithMastersCount = allOrders.Count(o => o.MasterId != null);

        decimal GetOrderRevenue(Order order)
        {
            if ((order.ClientTotal ?? 0m) > 0m)
                return order.ClientTotal!.Value;

            var completedWorksTotal = order.CompletedWorks?.Sum(cw => cw.WorkTotal) ?? 0m;
            var consumablesSellTotal = order.OrderConsumables?.Sum(oc => oc.Consumable.SellPrice * oc.Quantity) ?? 0m;
            var total = completedWorksTotal + consumablesSellTotal;

            if ((order.DiscountPercent ?? 0m) > 0m)
                total *= 1m - order.DiscountPercent!.Value / 100m;

            return Math.Round(total, 2);
        }

        decimal GetOrderPayouts(Order order)
        {
            var payouts = order.CompletedJobsPayouts?.Sum(p => p.Amount) ?? 0m;
            return payouts > 0m ? payouts : order.LaborCost ?? 0m;
        }

        decimal GetOrderConsumablesCost(Order order)
        {
            if ((order.ConsumablesCost ?? 0m) > 0m)
                return order.ConsumablesCost!.Value;

            return order.OrderConsumables?.Sum(oc => oc.Consumable.CostPrice * oc.Quantity) ?? 0m;
        }

        var revenueOrders = periodOrders.Where(o => GetOrderRevenue(o) > 0m).ToList();
        var totalRevenue = revenueOrders.Sum(GetOrderRevenue);
        var totalExpenses = revenueOrders.Sum(o => GetOrderPayouts(o) + GetOrderConsumablesCost(o));

        ViewBag.TodayRevenue = totalRevenue;
        ViewBag.TodayProfit = totalRevenue - totalExpenses;
        ViewBag.AverageCheck = revenueOrders.Count > 0 ? revenueOrders.Average(GetOrderRevenue) : 0m;

        var discountOrders = periodOrders
            .Where(o => (o.DiscountPercent ?? 0m) > 0m && GetOrderRevenue(o) > 0m)
            .ToList();

        decimal CalculateDiscountAmount(Order order)
        {
            var revenue = GetOrderRevenue(order);
            var discountPercent = order.DiscountPercent ?? 0m;
            if (revenue <= 0m || discountPercent <= 0m || discountPercent >= 100m)
                return 0m;

            var totalBeforeDiscount = revenue / (1m - discountPercent / 100m);
            return Math.Round(totalBeforeDiscount - revenue, 2);
        }

        ViewBag.DiscountStats = new
        {
            OrdersWithDiscount = discountOrders.Count,
            SoftDiscountOrders = discountOrders.Count(o => o.DiscountType == "soft"),
            HardDiscountOrders = discountOrders.Count(o => o.DiscountType == "hard"),
            MonthSoftLoss = discountOrders
                .Where(o => o.DiscountType == "soft")
                .Sum(CalculateDiscountAmount),
            MonthHardSavings = discountOrders
                .Where(o => o.DiscountType == "hard")
                .Sum(CalculateDiscountAmount)
        };

        var topClients = await _context.Clients
            .AsNoTracking()
            .Select(c => new
            {
                Client = c,
                OrderCount = _context.Cars
                    .Where(car => car.ClientId == c.ClientId)
                    .SelectMany(car => car.Orders)
                    .Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(3)
            .ToListAsync();
        ViewBag.TopClients = topClients;

        ViewBag.RecentOrders = await _context.Orders
            .Include(o => o.Car)
                .ThenInclude(c => c!.Client)
            .Include(o => o.Master)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        ViewBag.ActivePosts = await _context.Posts
            .Include(p => p.ActiveSessions!)
                .ThenInclude(s => s.Master)
            .OrderBy(p => p.PostId)
            .ToListAsync();

        ViewBag.PostsCount = ((IEnumerable<Post>)ViewBag.ActivePosts)
            .Count(p => p.ActiveSessions != null && p.ActiveSessions.Any(s => s.EndedAt == null));

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

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
