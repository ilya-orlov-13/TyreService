using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Services
{
    public class DistributionService : IDistributionService
    {
        private readonly ApplicationDbContext _context;

        public DistributionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DistributionResult> DistributeOrders(DateTime date)
        {
            var result = new DistributionResult
            {
                Date = date.Date
            };

            var orders = await _context.Orders
                .Include(o => o.Car).ThenInclude(c => c.Client)
                .Include(o => o.CompletedWorks).ThenInclude(cw => cw.Service)
                .Where(o => o.Status == "Новый"
                    && o.ScheduledAt.HasValue
                    && o.ScheduledAt.Value.Date == date.Date)
                .OrderBy(o => o.ScheduledAt)
                .ToListAsync();

            if (orders.Count == 0)
            {
                result.UnassignedOrders = orders;
                return result;
            }

            var freePosts = await _context.Posts
                .Where(p => !p.IsLocked)
                .OrderBy(p => p.PostId)
                .ToListAsync();

            if (freePosts.Count == 0)
            {
                result.UnassignedOrders = orders;
                return result;
            }

            foreach (var post in freePosts)
            {
                result.Posts.Add(new PostDistribution
                {
                    PostId = post.PostId,
                    PostName = post.Name
                });
            }

            var ordersQueue = new Queue<Order>(orders);
            var layers = new List<List<Order>>();
            var currentLayer = new List<Order>();
            var perPost = (int)Math.Ceiling((double)orders.Count / freePosts.Count);
            var count = 0;

            foreach (var order in orders)
            {
                currentLayer.Add(order);
                count++;

                if (currentLayer.Count >= perPost || count >= orders.Count)
                {
                    layers.Add(new List<Order>(currentLayer));
                    currentLayer.Clear();
                }
            }

            if (currentLayer.Count > 0)
                layers.Add(currentLayer);

            for (int i = 0; i < layers.Count; i++)
            {
                var postIndex = i % freePosts.Count;
                var postDist = result.Posts[postIndex];

                if (i == 0)
                    postDist.OrdersLayer1.AddRange(layers[i]);
                else if (i == 1)
                    postDist.OrdersLayer2.AddRange(layers[i]);
                else
                    postDist.OrdersLayer3.AddRange(layers[i]);
            }

            return result;
        }
    }
}
