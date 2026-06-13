using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/tires")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CustomerTiresController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CustomerTiresController(ApplicationDbContext db)
    {
        _db = db;
    }

    private int GetClientId() => CustomerClientIdResolver.Resolve(User, _db);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TireDto>>>> GetAll()
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<List<TireDto>>.Fail("Клиент не найден"));

        var tires = await _db.Tires
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.TireId)
            .ToListAsync();

        var dtos = tires.Select(ToDto).ToList();
        return Ok(ApiResponse<List<TireDto>>.Ok(dtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TireDto>>> GetById(int id)
    {
        var clientId = GetClientId();
        var tire = await _db.Tires
            .FirstOrDefaultAsync(t => t.TireId == id && t.ClientId == clientId);

        if (tire == null)
            return NotFound(ApiResponse<TireDto>.Fail("Шина не найдена"));

        return Ok(ApiResponse<TireDto>.Ok(ToDto(tire)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TireDto>>> Create([FromBody] CustomerCreateTireRequest request)
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<TireDto>.Fail("Клиент не найден"));

        if (string.IsNullOrWhiteSpace(request.Seasonality) ||
            string.IsNullOrWhiteSpace(request.Manufacturer) ||
            string.IsNullOrWhiteSpace(request.TireModel) ||
            string.IsNullOrWhiteSpace(request.Size))
            return BadRequest(ApiResponse<TireDto>.Fail("Заполните все обязательные поля"));

        var tire = new Tire
        {
            ClientId = clientId,
            TireType = string.IsNullOrWhiteSpace(request.TireType) ? "Легковая" : request.TireType.Trim(),
            Seasonality = request.Seasonality.Trim(),
            Manufacturer = request.Manufacturer.Trim(),
            TireModel = request.TireModel.Trim(),
            Size = request.Size.Trim(),
            LoadIndex = request.LoadIndex,
            WearPercentage = request.WearPercentage,
            Pressure = request.Pressure
        };

        _db.Tires.Add(tire);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tire.TireId },
            ApiResponse<TireDto>.Ok(ToDto(tire)));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TireDto>>> Update(int id, [FromBody] CustomerUpdateTireRequest request)
    {
        var clientId = GetClientId();
        var tire = await _db.Tires
            .FirstOrDefaultAsync(t => t.TireId == id && t.ClientId == clientId);

        if (tire == null)
            return NotFound(ApiResponse<TireDto>.Fail("Шина не найдена"));

        if (string.IsNullOrWhiteSpace(request.Seasonality) ||
            string.IsNullOrWhiteSpace(request.Manufacturer) ||
            string.IsNullOrWhiteSpace(request.TireModel) ||
            string.IsNullOrWhiteSpace(request.Size))
            return BadRequest(ApiResponse<TireDto>.Fail("Заполните все обязательные поля"));

        tire.TireType = string.IsNullOrWhiteSpace(request.TireType) ? "Легковая" : request.TireType.Trim();
        tire.Seasonality = request.Seasonality.Trim();
        tire.Manufacturer = request.Manufacturer.Trim();
        tire.TireModel = request.TireModel.Trim();
        tire.Size = request.Size.Trim();
        tire.LoadIndex = request.LoadIndex;
        tire.WearPercentage = request.WearPercentage;
        tire.Pressure = request.Pressure;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<TireDto>.Ok(ToDto(tire)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var clientId = GetClientId();
        var tire = await _db.Tires
            .FirstOrDefaultAsync(t => t.TireId == id && t.ClientId == clientId);

        if (tire == null)
            return NotFound(ApiResponse<object>.Fail("Шина не найдена"));

        _db.Tires.Remove(tire);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }));
    }

    private static TireDto ToDto(Tire tire)
    {
        return new TireDto(
            tire.TireId,
            tire.TireType,
            tire.Seasonality,
            tire.Manufacturer,
            tire.TireModel,
            tire.Size,
            tire.LoadIndex,
            tire.WearPercentage,
            tire.Pressure,
            tire.FullInfo
        );
    }
}
