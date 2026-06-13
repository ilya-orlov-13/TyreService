using TyreServiceApp.Models;

namespace TyreServiceApp.Services
{
    public interface IDistributionService
    {
        Task<DistributionResult> DistributeOrders(DateTime date);
    }

    public class DistributionResult
    {
        public DateTime Date { get; set; }
        public List<PostDistribution> Posts { get; set; } = new();
        public List<Order> UnassignedOrders { get; set; } = new();
    }

    public class PostDistribution
    {
        public int PostId { get; set; }
        public string PostName { get; set; } = string.Empty;
        public List<Order> OrdersLayer1 { get; set; } = new();
        public List<Order> OrdersLayer2 { get; set; } = new();
        public List<Order> OrdersLayer3 { get; set; } = new();
        public int TotalOrders => OrdersLayer1.Count + OrdersLayer2.Count + OrdersLayer3.Count;
    }
}
