using TyreServiceApp.Models;

namespace TyreServiceApp.Utils;

public static class OrderRevenueHelper
{
    public static DateTime GetRevenueDate(Order order)
    {
        if (order.PaymentDate.HasValue)
            return order.PaymentDate.Value.Date;

        var payoutDate = order.CompletedJobsPayouts?.Max(p => (DateTime?)p.CreatedAt);
        if (payoutDate.HasValue)
            return payoutDate.Value.Date;

        return order.OrderDate.Date;
    }

    public static decimal GetRevenue(Order order)
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

    public static decimal GetPayouts(Order order)
    {
        var payouts = order.CompletedJobsPayouts?.Sum(p => p.Amount) ?? 0m;
        return payouts > 0m ? payouts : order.LaborCost ?? 0m;
    }

    public static decimal GetConsumablesCost(Order order)
    {
        if ((order.ConsumablesCost ?? 0m) > 0m)
            return order.ConsumablesCost!.Value;

        return order.OrderConsumables?.Sum(oc => oc.Consumable.CostPrice * oc.Quantity) ?? 0m;
    }

    public static bool IsCompletedOrder(Order order, IReadOnlySet<int> completedOrderNumbers) =>
        completedOrderNumbers.Contains(order.OrderNumber);

    public static bool IsInPeriod(Order order, DateTime periodStart, DateTime periodEndExclusive) =>
        GetRevenueDate(order) >= periodStart && GetRevenueDate(order) < periodEndExclusive;
}
