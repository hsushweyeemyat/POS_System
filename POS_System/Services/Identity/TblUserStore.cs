using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS_System.Data.Contexts;
using POS_System.Data.Entities;

namespace POS_System.Services.Identity;

public class TblUserStore :
    IUserStore<TblUser>,
    IUserPasswordStore<TblUser>,
    IUserEmailStore<TblUser>,
    IUserRoleStore<TblUser>
{
    private readonly ApplicationDbContext _dbContext;

    public TblUserStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<string> GetUserIdAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.Email);
    }

    public Task SetUserNameAsync(TblUser user, string? userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(Normalize(user.Email));
    }

    public Task SetNormalizedUserNameAsync(TblUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _dbContext.TblUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _dbContext.TblUsers.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _dbContext.TblUsers.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<TblUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!int.TryParse(userId, out var parsedUserId))
        {
            return null;
        }

        return await _dbContext.TblUsers
            .SingleOrDefaultAsync(user => user.Id == parsedUserId, cancellationToken);
    }

    public async Task<TblUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dbContext.TblUsers
            .SingleOrDefaultAsync(
                user => user.Email.ToUpper() == normalizedUserName,
                cancellationToken);
    }

    public Task SetPasswordHashAsync(TblUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    public Task SetEmailAsync(TblUser user, string? email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public Task SetEmailConfirmedAsync(TblUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public async Task<TblUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dbContext.TblUsers
            .SingleOrDefaultAsync(
                user => user.Email.ToUpper() == normalizedEmail,
                cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(Normalize(user.Email));
    }

    public Task SetNormalizedEmailAsync(TblUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task AddToRoleAsync(TblUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.Role = roleName;
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(TblUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase))
        {
            user.Role = string.Empty;
        }

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(TblUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IList<string> roles = string.IsNullOrWhiteSpace(user.Role)
            ? new List<string>()
            : new List<string> { user.Role };

        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(TblUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IList<TblUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var users = await _dbContext.TblUsers
            .Where(user => user.Role == roleName)
            .ToListAsync(cancellationToken);

        return users;
    }

    public void Dispose()
    {
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).ToUpperInvariant();
    }
}
