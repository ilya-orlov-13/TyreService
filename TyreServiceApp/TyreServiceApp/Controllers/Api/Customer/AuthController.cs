using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Customer.Models;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(ApplicationDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    private static string NormalizePhone(string phone) =>
        string.Concat(phone?.Where(char.IsDigit) ?? []);

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var phone = NormalizePhone(request.Phone);
        var all = await _db.Set<CustomerUser>()
            .Include(u => u.Client)
            .ToListAsync();
        var user = all.FirstOrDefault(u => NormalizePhone(u.Phone) == phone);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Pin, user.PinHash))
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Неверный телефон или PIN-код"));

        var clientId = await EnsureClientIdAsync(user, phone);
        await _db.Entry(user).Reference(u => u.Client).LoadAsync();

        var token = _jwt.GenerateToken(
            user.Id,
            user.Phone,
            user.Client?.FullName ?? user.Phone,
            clientId
        );

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse(
            token,
            new UserDto(
                user.Id,
                clientId,
                user.Client?.FullName ?? user.Phone,
                user.Phone
            )
        )));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var phone = NormalizePhone(request.Phone);
        var allClients = await _db.Clients.ToListAsync();
        var existing = allClients.Any(c => NormalizePhone(c.Phone) == phone);
        if (existing)
            return Conflict(ApiResponse<AuthResponse>.Fail("Этот телефон уже зарегистрирован"));

        var client = new Client
        {
            FullName = request.FullName,
            Phone = phone
        };
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        var user = new CustomerUser
        {
            Phone = phone,
            PinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin),
            ClientId = client.ClientId
        };
        _db.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(
            user.Id,
            user.Phone,
            client.FullName,
            client.ClientId
        );

        return Created("", ApiResponse<AuthResponse>.Ok(new AuthResponse(
            token,
            new UserDto(
                user.Id,
                client.ClientId,
                client.FullName,
                user.Phone
            )
        )));
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me()
    {
        var customerIdClaim = User.FindFirst("CustomerId")?.Value;
        if (!int.TryParse(customerIdClaim, out var customerId))
            return Unauthorized(ApiResponse<UserDto>.Fail("Неверный токен"));

        var user = await _db.Set<CustomerUser>()
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Id == customerId);

        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("Пользователь не найден"));

        var clientId = user.ClientId ?? 0;
        if (clientId == 0)
            clientId = await EnsureClientIdAsync(user, NormalizePhone(user.Phone));

        return Ok(ApiResponse<UserDto>.Ok(new UserDto(
            user.Id,
            clientId,
            user.Client?.FullName ?? user.Phone,
            user.Phone
        )));
    }

    private async Task<int> EnsureClientIdAsync(CustomerUser user, string phone)
    {
        if (user.ClientId is > 0)
            return user.ClientId.Value;

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Phone == phone);
        if (client == null)
        {
            client = new Client
            {
                FullName = user.Client?.FullName ?? phone,
                Phone = phone
            };
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();
        }

        user.ClientId = client.ClientId;
        await _db.SaveChangesAsync();
        return client.ClientId;
    }
}
