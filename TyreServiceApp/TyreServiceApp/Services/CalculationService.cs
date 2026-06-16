using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ApplicationDbContext _context;

        public CalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CalculateOrderTotal(int orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .Include(o => o.OrderComplexities)
                    .ThenInclude(oc => oc.ComplexityCoefficient)
                .Include(o => o.OrderConsumables)
                    .ThenInclude(oc => oc.Consumable)
                .Include(o => o.Car)
                    .ThenInclude(c => c.CarClass)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) return;

            var carClassId = order.Car?.CarClassId;
            var complexitySum = order.OrderComplexities?
                .Where(oc => oc.ComplexityCoefficient?.IsActive == true)
                .Sum(oc => oc.ComplexityCoefficient?.Factor ?? 0m) ?? 0m;
            if (complexitySum <= 0) complexitySum = 1m;

            var totalClient = 0m;
            var totalMasterShare = 0m;

            if (order.CompletedWorks != null)
            {
                foreach (var cw in order.CompletedWorks.Where(cw => cw.CompletionTimeMin > 0))
                {
                    var basePrice = await GetDefaultTariff(cw.ServiceCode, carClassId);
                    var lineTotal = basePrice * complexitySum * cw.WheelCount;
                    cw.WorkTotal = lineTotal;
                    totalClient += lineTotal;
                }
            }

            if (order.OrderConsumables != null)
            {
                foreach (var oc in order.OrderConsumables)
                {
                    totalClient += oc.Consumable.SellPrice * oc.Quantity;
                }
            }

            if (order.DiscountPercent.HasValue && order.DiscountPercent > 0)
            {
                var factor = 1m - order.DiscountPercent.Value / 100m;
                totalClient *= factor;
            }

            order.ClientTotal = totalClient;
            await _context.SaveChangesAsync();
        }

        public async Task<List<CompletedJobsPayout>> CalculateMasterPayout(int orderNumber, List<int> masterIds)
        {
            var order = await _context.Orders
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .Include(o => o.OrderComplexities)
                    .ThenInclude(oc => oc.ComplexityCoefficient)
                .Include(o => o.Car)
                    .ThenInclude(c => c.CarClass)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null || masterIds.Count == 0)
                return new List<CompletedJobsPayout>();

            var carClassId = order.Car?.CarClassId;
            var complexitySum = order.OrderComplexities?
                .Where(oc => oc.ComplexityCoefficient?.IsActive == true)
                .Sum(oc => oc.ComplexityCoefficient?.Factor ?? 0m) ?? 0m;
            if (complexitySum <= 0) complexitySum = 1m;

            var workIds = order.CompletedWorks?
                .Where(cw => cw.CompletionTimeMin > 0)
                .Select(cw => cw.WorkId)
                .ToList() ?? new();

            var timeLogs = await _context.WorkTimeLogs
                .Where(w => workIds.Contains(w.WorkId))
                .ToListAsync();

            var minutesPerMaster = timeLogs
                .GroupBy(w => w.MasterId)
                .ToDictionary(g => g.Key, g => g.Sum(w => w.DurationMinutes));

            var totalMasterShare = 0m;
            var masterShares = masterIds.ToDictionary(mid => mid, mid => 0m);

            if (order.CompletedWorks != null)
            {
                foreach (var cw in order.CompletedWorks.Where(cw => cw.CompletionTimeMin > 0))
                {
                    var basePrice = await GetDefaultTariff(cw.ServiceCode, carClassId);
                    var masterPercent = await GetMasterSharePercent(cw.ServiceCode, carClassId);
                    var lineShare = basePrice * masterPercent / 100m * complexitySum * cw.WheelCount;

                    var cwLogs = timeLogs.Where(w => w.WorkId == cw.WorkId).ToList();
                    var totalMinutes = cwLogs.Sum(w => w.DurationMinutes);

                    if (totalMinutes > 0 && masterIds.Count > 0)
                    {
                        var perMasterMinutes = cwLogs
                            .GroupBy(w => w.MasterId)
                            .ToDictionary(g => g.Key, g => g.Sum(w => w.DurationMinutes));

                        foreach (var mid in masterIds)
                        {
                            var masterMin = perMasterMinutes.GetValueOrDefault(mid, 0);
                            var share = lineShare * masterMin / totalMinutes;
                            masterShares[mid] += share;
                        }
                    }
                    else
                    {
                        var avg = lineShare / masterIds.Count;
                        foreach (var mid in masterIds)
                            masterShares[mid] += avg;
                    }

                    totalMasterShare += lineShare;
                }
            }

            if (order.DiscountPercent.HasValue
                && order.DiscountPercent > 0
                && order.DiscountType == "hard")
            {
                var factor = 1m - order.DiscountPercent.Value / 100m;
                totalMasterShare *= factor;
                foreach (var mid in masterIds)
                    masterShares[mid] *= factor;
            }

            order.LaborCost = totalMasterShare;

            var payouts = new List<CompletedJobsPayout>();
            foreach (var mid in masterIds)
            {
                payouts.Add(new CompletedJobsPayout
                {
                    OrderNumber = orderNumber,
                    MasterId = mid,
                    Amount = Math.Round(masterShares[mid], 2),
                    CreatedAt = PermTime.Now
                });
            }

            _context.CompletedJobsPayouts.AddRange(payouts);
            await _context.SaveChangesAsync();

            return payouts;
        }

        public async Task<RevenueReportRow> CalculateOwnerRevenue(int orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.Car).ThenInclude(c => c.Client)
                .Include(o => o.CompletedWorks)
                .Include(o => o.OrderConsumables).ThenInclude(oc => oc.Consumable)
                .Include(o => o.CompletedJobsPayouts)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
                return new RevenueReportRow { OrderNumber = orderNumber };

            var clientTotal = order.ClientTotal ?? 0m;
            var masterShare = order.LaborCost ?? 0m;
            var consumablesCost = order.OrderConsumables?
                .Sum(oc => oc.Consumable.CostPrice * oc.Quantity) ?? 0m;

            var settings = await _context.OwnerSettings.FirstOrDefaultAsync();
            var acquiringPercent = settings?.AcquiringFeePercent ?? 2m;
            var taxPercent = settings?.TaxPercent ?? 6m;

            var acquiringFee = clientTotal * acquiringPercent / 100m;
            var taxes = clientTotal * taxPercent / 100m;
            var netProfit = clientTotal - masterShare - consumablesCost - acquiringFee - taxes;

            var carInfo = order.Car != null
                ? $"{order.Car.Brand} {order.Car.Model} ({order.Car.LicensePlate})"
                : "";
            var clientName = order.Car?.Client?.FullName ?? "";

            return new RevenueReportRow
            {
                OrderNumber = orderNumber,
                ClientName = clientName,
                CarInfo = carInfo,
                ClientTotal = clientTotal,
                MasterShare = masterShare,
                ConsumablesCost = consumablesCost,
                AcquiringFee = Math.Round(acquiringFee, 2),
                Taxes = Math.Round(taxes, 2),
                NetProfit = Math.Round(netProfit, 2)
            };
        }

        public async Task<decimal> GetDefaultTariff(int serviceCode, int? carClassId)
        {
            if (carClassId.HasValue)
            {
                var tariff = await _context.ServiceTariffs
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && st.CarClassId == carClassId.Value);

                if (tariff != null)
                    return tariff.BasePrice;
            }

            var service = await _context.Services.FindAsync(serviceCode);
            return service?.ServiceCost ?? 0m;
        }

        public async Task<int> GetAvailableEarlyMinutes(DateTime date)
        {
            var totalSaved = await _context.CompletedWorks
                .Where(cw => cw.TimeSavedMin > 0
                    && cw.Order != null
                    && cw.Order.OrderDate.Date == date.Date)
                .SumAsync(cw => (int?)cw.TimeSavedMin) ?? 0;

            return totalSaved;
        }

        public async Task<List<SpeedBonus>> CalculateMonthlySpeedBonuses(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var bonuses = new List<SpeedBonus>();

            var masterGroups = await _context.CompletedWorks
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .Where(cw => cw.TimeSavedMin > 0
                    && cw.MasterId.HasValue
                    && cw.Order != null
                    && cw.Order.OrderDate >= monthStart
                    && cw.Order.OrderDate < monthEnd)
                .GroupBy(cw => cw.MasterId)
                .ToListAsync();

            foreach (var group in masterGroups)
            {
                var masterId = group.Key!.Value;
                var totalSavedMin = group.Sum(cw => cw.TimeSavedMin);
                var master = group.First().Master;

                var bonusAmount = Math.Round(totalSavedMin / 60m * (master?.HourlyRate ?? 0m) * 0.5m, 2);

                if (bonusAmount > 0)
                {
                    var existing = await _context.SpeedBonuses
                        .FirstOrDefaultAsync(sb => sb.MasterId == masterId
                            && sb.CreatedAt >= monthStart
                            && sb.CreatedAt < monthEnd);

                    if (existing == null)
                    {
                        var monthlyBonus = new SpeedBonus
                        {
                            MasterId = masterId,
                            OrderNumber = 0,
                            WorkId = 0,
                            TimeSavedMin = totalSavedMin,
                            BonusAmount = bonusAmount,
                            CreatedAt = PermTime.Now
                        };
                        _context.SpeedBonuses.Add(monthlyBonus);
                        bonuses.Add(monthlyBonus);
                    }
                }
            }

            if (bonuses.Any())
                await _context.SaveChangesAsync();

            return bonuses;
        }

        private async Task<decimal> GetMasterSharePercent(int serviceCode, int? carClassId)
        {
            if (carClassId.HasValue)
            {
                var tariff = await _context.ServiceTariffs
                    .FirstOrDefaultAsync(st => st.ServiceCode == serviceCode && st.CarClassId == carClassId.Value);

                if (tariff != null)
                    return tariff.MasterSharePercent;
            }

            return 40m;
        }
    }
}
