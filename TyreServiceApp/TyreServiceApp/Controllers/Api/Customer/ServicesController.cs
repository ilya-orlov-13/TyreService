using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/services")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ServicesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ServicesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> GetAll()
    {
        var services = await _db.Services
            .Where(s => !s.IsConsultation)
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceDto(
                s.ServiceCode,
                s.ServiceName,
                s.ServiceCost,
                s.FixedDurationMin
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<ServiceDto>>.Ok(services));
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> GetByCode(int code)
    {
        var service = await _db.Services
            .Where(s => !s.IsConsultation)
            .FirstOrDefaultAsync(s => s.ServiceCode == code);

        if (service == null)
            return NotFound(ApiResponse<ServiceDto>.Fail("Услуга не найдена"));

        return Ok(ApiResponse<ServiceDto>.Ok(new ServiceDto(
            service.ServiceCode,
            service.ServiceName,
            service.ServiceCost,
            service.FixedDurationMin
        )));
    }
}
