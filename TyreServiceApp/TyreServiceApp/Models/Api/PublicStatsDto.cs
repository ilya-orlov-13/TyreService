namespace TyreServiceApp.Models.Api;

public class PublicStatsDto
{
    public int CarsServed { get; set; }
    public int OrdersTotal { get; set; }
    public int ClientsTotal { get; set; }
    public decimal? SatisfactionPercent { get; set; }
    public int BranchesCount { get; set; }
    public bool IsOpen24h { get; set; } = true;
}
