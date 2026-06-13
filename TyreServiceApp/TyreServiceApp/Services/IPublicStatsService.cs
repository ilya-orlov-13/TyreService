using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Services;

public interface IPublicStatsService
{
    Task<PublicStatsDto> GetStatsAsync();
}
