using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICalculationService _calc;

        public ReportsController(ApplicationDbContext context, ICalculationService calc)
        {
            _context = context;
            _calc = calc;
        }

        public async Task<IActionResult> Revenue(DateTime? dateFrom, DateTime? dateTo)
        {
            dateFrom ??= new DateTime(PermTime.Today.Year, PermTime.Today.Month, 1);
            dateTo ??= PermTime.Today;

            var orders = await _context.Orders
                .Include(o => o.Car).ThenInclude(c => c.Client)
                .Include(o => o.OrderConsumables).ThenInclude(oc => oc.Consumable)
                .Include(o => o.CompletedJobsPayouts)
                .Where(o => o.OrderDate >= dateFrom.Value && o.OrderDate <= dateTo.Value.AddDays(1))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var rows = new List<RevenueReportRow>();
            foreach (var order in orders)
            {
                rows.Add(await _calc.CalculateOwnerRevenue(order.OrderNumber));
            }

            ViewBag.DateFrom = dateFrom.Value.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo.Value.ToString("yyyy-MM-dd");
            return View(rows);
        }
    }
}
