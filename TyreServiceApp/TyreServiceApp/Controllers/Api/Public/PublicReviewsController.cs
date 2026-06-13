using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Controllers.Api.Public;

[Route("api/public/reviews")]
[ApiController]
public class PublicReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PublicReviewsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetAll([FromQuery] int limit = 12)
    {
        if (limit < 1) limit = 12;
        if (limit > 50) limit = 50;

        var reviews = await _db.CustomerReviews
            .Where(r => r.IsApproved)
            .Include(r => r.Customer)
            .ThenInclude(c => c!.Client)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Author = r.Customer!.Client != null ? r.Customer.Client.FullName : r.Customer.Phone,
                Rating = r.Rating,
                Text = r.Text,
                CarModel = r.CarModel,
                OrderNumber = r.OrderNumber,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ReviewDto>>.Ok(reviews));
    }
}
