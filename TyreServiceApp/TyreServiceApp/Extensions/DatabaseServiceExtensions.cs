using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;

namespace TyreServiceApp.Extensions;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        if (IsSqlServerConnection(connectionString))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(EnsurePostgresPassword(connectionString, configuration)));

        return services;
    }

    private static bool IsSqlServerConnection(string connectionString) =>
        connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase);

    private static string EnsurePostgresPassword(string connectionString, IConfiguration configuration)
    {
        if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var dbPassword = configuration["DbPassword"]
            ?? throw new InvalidOperationException("Configuration value 'DbPassword' is required for PostgreSQL connections.");

        return $"{connectionString}Password={dbPassword};";
    }
}
