using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Services;

public class PublicStatsService : IPublicStatsService
{
    private readonly ApplicationDbContext _db;

    public PublicStatsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PublicStatsDto> GetStatsAsync()
    {
        var carsServed = await _db.Cars.CountAsync();
        var ordersTotal = await _db.Orders.CountAsync();
        var clientsTotal = await _db.Clients.CountAsync();
        var postsCount = await _db.Posts.CountAsync();

        decimal? satisfaction = null;
        if (await _db.CustomerReviews.AnyAsync())
        {
            var avgRating = await _db.CustomerReviews.AverageAsync(r => (double)r.Rating);
            satisfaction = Math.Round((decimal)(avgRating / 5.0 * 100.0), 1);
        }

        return new PublicStatsDto
        {
            CarsServed = carsServed,
            OrdersTotal = ordersTotal,
            ClientsTotal = clientsTotal,
            SatisfactionPercent = satisfaction,
            BranchesCount = postsCount > 0 ? postsCount : 3,
            IsOpen24h = true
        };
    }
}
