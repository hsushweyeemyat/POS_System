using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS_System.Data.Contexts;

namespace POS_System.Services.Identity;

public class ActiveUserCookieEvents : CookieAuthenticationEvents
{
    private readonly ApplicationDbContext _dbContext;

    public ActiveUserCookieEvents(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userIdValue = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            await RejectPrincipalAsync(context);
            return;
        }

        var user = await _dbContext.TblUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId, context.HttpContext.RequestAborted);

        if (user is null || user.IsActive == 0)
        {
            await RejectPrincipalAsync(context);
            return;
        }

        var currentRole = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.Equals(currentRole, user.Role, StringComparison.OrdinalIgnoreCase))
        {
            await RejectPrincipalAsync(context);
        }
    }

    private static async Task RejectPrincipalAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}
