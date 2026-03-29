using Microsoft.EntityFrameworkCore;
using POS_System.Data.Contexts;
using POS_System.Data.Entities;
using POS_System.Extensions;
using POS_System.Repositories.Interfaces;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Implementations;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserManagementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TblUser>> GetPagedAsync(
        string? searchTerm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblUsers
            .AsNoTracking()
            .Where(user => user.IsActive == 1);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

            query = query.Where(user =>
                EF.Functions.Like(user.FullName, pattern) ||
                EF.Functions.Like(user.Email, pattern) ||
                EF.Functions.Like(user.Role, pattern));
        }

        return await query
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Select(user => new TblUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            })
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<TblUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TblUsers
            .AsNoTracking()
            .Where(user => user.Id == id && user.IsActive == 1)
            .Select(user => new TblUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
}
