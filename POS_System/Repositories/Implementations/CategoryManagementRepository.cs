using Microsoft.EntityFrameworkCore;
using POS_System.Data.Contexts;
using POS_System.Data.Entities;
using POS_System.Extensions;
using POS_System.Repositories.Interfaces;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Implementations;

public class CategoryManagementRepository : ICategoryManagementRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryManagementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TblCategory>> GetPagedAsync(
        string? searchTerm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblCategories
            .AsNoTracking()
            .Where(category => category.IsActive == 1);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

            query = query.Where(category => EF.Functions.Like(category.Name, pattern));
        }

        return await query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Select(category => new TblCategory
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate
            })
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task<TblCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TblCategories
            .AsNoTracking()
            .Where(category => category.Id == id && category.IsActive == 1)
            .Select(category => new TblCategory
            {
                Id = category.Id,
                Name = category.Name,
                IsActive = category.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TblCategory?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TblCategories
            .SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblCategories
            .AsNoTracking()
            .Where(category => category.IsActive == 1 && category.Name == name);

        if (excludingId.HasValue)
        {
            query = query.Where(category => category.Id != excludingId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public Task<bool> HasActiveProductsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TblProducts
            .AsNoTracking()
            .AnyAsync(product => product.CategoryId == categoryId && product.IsActive == 1, cancellationToken);
    }

    public async Task AddAsync(TblCategory category, CancellationToken cancellationToken = default)
    {
        await _dbContext.TblCategories.AddAsync(category, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
}
