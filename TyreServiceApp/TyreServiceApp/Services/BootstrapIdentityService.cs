using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Services;

public class BootstrapIdentityService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BootstrapIdentityService> _logger;

    public BootstrapIdentityService(
        ApplicationDbContext db,
        IConfiguration configuration,
        ILogger<BootstrapIdentityService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureBootstrapUsersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAsync(cancellationToken);
        await EnsureAdminAsync(cancellationToken);
    }

    private async Task EnsureOwnerAsync(CancellationToken cancellationToken)
    {
        if (await _db.OwnerUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var login = _configuration["Bootstrap:OwnerLogin"];
        var password = _configuration["Bootstrap:OwnerPassword"];

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var owner = new OwnerUser
        {
            Login = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = _configuration["Bootstrap:OwnerFullName"],
            Email = _configuration["Bootstrap:OwnerEmail"]
        };

        _db.OwnerUsers.Add(owner);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bootstrap owner user '{Login}' created.", login);
    }

    private async Task EnsureAdminAsync(CancellationToken cancellationToken)
    {
        if (await _db.AdminUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var login = _configuration["Bootstrap:AdminLogin"];
        var password = _configuration["Bootstrap:AdminPassword"];

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var admin = new AdminUser
        {
            Login = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = _configuration["Bootstrap:AdminFullName"],
            Email = _configuration["Bootstrap:AdminEmail"]
        };

        _db.AdminUsers.Add(admin);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bootstrap admin user '{Login}' created.", login);
    }
}
