using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Customer.Models;
using TyreServiceApp.Data;

namespace TyreServiceApp.Utils;

public static class CustomerClientIdResolver
{
    public static int Resolve(ClaimsPrincipal user, ApplicationDbContext db)
    {
        var claim = user.FindFirst("ClientId")?.Value;
        if (int.TryParse(claim, out var id) && id > 0)
            return id;

        var customerIdClaim = user.FindFirst("CustomerId")?.Value;
        if (!int.TryParse(customerIdClaim, out var customerId))
            return 0;

        var clientId = db.Set<CustomerUser>()
            .AsNoTracking()
            .Where(u => u.Id == customerId)
            .Select(u => u.ClientId)
            .FirstOrDefault();

        return clientId ?? 0;
    }
}
