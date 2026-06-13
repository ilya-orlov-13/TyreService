using Microsoft.AspNetCore.Mvc;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;

namespace TyreServiceApp.Controllers.Api.Public;

[Route("api/public/stats")]
[ApiController]
public class PublicStatsController : ControllerBase
{
    private readonly IPublicStatsService _stats;

    public PublicStatsController(IPublicStatsService stats)
    {
        _stats = stats;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PublicStatsDto>>> Get()
    {
        var data = await _stats.GetStatsAsync();
        return Ok(ApiResponse<PublicStatsDto>.Ok(data));
    }
}
