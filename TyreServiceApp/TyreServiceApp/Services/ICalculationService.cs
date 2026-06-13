using TyreServiceApp.Models;

namespace TyreServiceApp.Services
{
    public interface ICalculationService
    {
        Task CalculateOrderTotal(int orderNumber);
        Task<List<CompletedJobsPayout>> CalculateMasterPayout(int orderNumber, List<int> masterIds);
        Task<decimal> GetDefaultTariff(int serviceCode, int? carClassId);
        Task<RevenueReportRow> CalculateOwnerRevenue(int orderNumber);
        Task<int> GetAvailableEarlyMinutes(DateTime date);
        Task<List<SpeedBonus>> CalculateMonthlySpeedBonuses(int year, int month);
    }

    public class RevenueReportRow
    {
        public int OrderNumber { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string CarInfo { get; set; } = string.Empty;
        public decimal ClientTotal { get; set; }
        public decimal MasterShare { get; set; }
        public decimal ConsumablesCost { get; set; }
        public decimal AcquiringFee { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetProfit { get; set; }
    }
}
