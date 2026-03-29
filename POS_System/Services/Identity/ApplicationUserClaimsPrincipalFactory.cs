using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using POS_System.Data.Entities;

namespace POS_System.Services.Identity;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<TblUser>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<TblUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TblUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("full_name", user.FullName));

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
        }

        return identity;
    }
}
