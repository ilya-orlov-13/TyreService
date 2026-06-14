using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Customer.Models;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/reviews")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CustomerReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomerReviewsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private int? GetCustomerId()
    {
        var claim = User.FindFirst("CustomerId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    private int? GetClientId(int customerId)
    {
        var user = _db.Set<CustomerUser>()
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == customerId);
        return user?.ClientId;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetMine()
    {
        var customerId = GetCustomerId();
        if (customerId == null)
            return Unauthorized(ApiResponse<List<ReviewDto>>.Fail("Неверный токен"));

        var user = await _db.Set<CustomerUser>()
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Id == customerId);
        var author = user?.Client?.FullName ?? user?.Phone ?? "";

        var reviews = await _db.CustomerReviews
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Author = author,
                Rating = r.Rating,
                Text = r.Text,
                CarModel = r.CarModel,
                OrderNumber = r.OrderNumber,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ReviewDto>>.Ok(reviews));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateOrUpdate([FromBody] CreateReviewRequest request)
    {
        var customerId = GetCustomerId();
        if (customerId == null)
            return Unauthorized(ApiResponse<ReviewDto>.Fail("Неверный токен"));

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(ApiResponse<ReviewDto>.Fail("Рейтинг должен быть от 1 до 5"));

        var text = request.Text?.Trim() ?? "";
        if (text.Length < 10 || text.Length > 1000)
            return BadRequest(ApiResponse<ReviewDto>.Fail("Текст отзыва: от 10 до 1000 символов"));

        var user = await _db.Set<CustomerUser>()
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Id == customerId);
        if (user == null)
            return NotFound(ApiResponse<ReviewDto>.Fail("Пользователь не найден"));

        if (request.OrderNumber.HasValue)
        {
            var clientId = user.ClientId;
            if (clientId == null)
                return BadRequest(ApiResponse<ReviewDto>.Fail("Клиент не найден"));

            var order = await _db.Orders
                .Include(o => o.Car)
                .FirstOrDefaultAsync(o =>
                    o.OrderNumber == request.OrderNumber.Value &&
                    (o.Car != null ? o.Car.ClientId == clientId : o.Tire!.ClientId == clientId));

            if (order == null)
                return NotFound(ApiResponse<ReviewDto>.Fail("Заказ не найден"));

            if (order.Status != "Готов" && order.Status != "Оплачено")
                return BadRequest(ApiResponse<ReviewDto>.Fail("Отзыв можно оставить только к выполненному заказу"));

            var existingForOrder = await _db.CustomerReviews
                .AnyAsync(r => r.CustomerId == customerId && r.OrderNumber == request.OrderNumber);

            if (existingForOrder)
                return BadRequest(ApiResponse<ReviewDto>.Fail("Отзыв на этот заказ уже оставлен"));
        }

        var existing = await _db.CustomerReviews
            .FirstOrDefaultAsync(r => r.CustomerId == customerId && !r.OrderNumber.HasValue);

        CustomerReview review;
        if (existing != null && !request.OrderNumber.HasValue)
        {
            existing.Rating = request.Rating;
            existing.Text = text;
            existing.CarModel = string.IsNullOrWhiteSpace(request.CarModel) ? null : request.CarModel.Trim();
            existing.UpdatedAt = PermTime.Now;
            review = existing;
        }
        else
        {
            review = new CustomerReview
            {
                CustomerId = customerId.Value,
                Rating = request.Rating,
                Text = text,
                CarModel = string.IsNullOrWhiteSpace(request.CarModel) ? null : request.CarModel.Trim(),
                OrderNumber = request.OrderNumber,
                CreatedAt = PermTime.Now
            };
            _db.CustomerReviews.Add(review);
        }

        await _db.SaveChangesAsync();

        var author = user.Client?.FullName ?? user.Phone;
        return Ok(ApiResponse<ReviewDto>.Ok(ToDto(review, author)));
    }

    private static ReviewDto ToDto(CustomerReview r, string author) => new()
    {
        ReviewId = r.ReviewId,
        Author = author,
        Rating = r.Rating,
        Text = r.Text,
        CarModel = r.CarModel,
        OrderNumber = r.OrderNumber,
        CreatedAt = r.CreatedAt
    };
}
