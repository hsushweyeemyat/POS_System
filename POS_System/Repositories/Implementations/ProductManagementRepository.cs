namespace POS_System.Repositories.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using POS_System.Data.Contexts;
    using POS_System.Data.Entities;
    using POS_System.Extensions;
    using POS_System.Repositories.Interfaces;
    using POS_System.ViewModels.Shared;

    public class ProductManagementRepository : IProductManagementRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductManagementRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<TblProduct>> GetPagedAsync(
            string? searchTerm,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TblProducts
                .AsNoTracking()
                .Where(product => product.IsActive == 1);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

                query = query.Where(product =>
                    EF.Functions.Like(product.Name, pattern) ||
                    EF.Functions.Like(product.Category!.Name, pattern));
            }

            return await query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
                .Select(product => new TblProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    StockQty = product.StockQty,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive,
                    CreatedDate = product.CreatedDate,
                    Category = product.Category == null
                        ? null
                        : new TblCategory
                        {
                            Id = product.Category.Id,
                            Name = product.Category.Name
                        }
                })
                .ToPagedResultAsync(pagination, cancellationToken);
        }

        public async Task<TblProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TblProducts
                .AsNoTracking()
                .Where(product => product.Id == id && product.IsActive == 1)
                .Select(product => new TblProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    StockQty = product.StockQty,
                    CategoryId = product.CategoryId,
                    IsActive = product.IsActive
                })
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<TblProduct?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TblProducts
                .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<TblCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.TblCategories
                .AsNoTracking()
                .Where(category => category.IsActive == 1)
                .OrderBy(category => category.Name)
                .ThenBy(category => category.Id)
                .Select(category => new TblCategory
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(
            string name,
            int? excludingId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TblProducts
                .AsNoTracking()
                .Where(product => product.IsActive == 1 && product.Name == name);

            if (excludingId.HasValue)
            {
                query = query.Where(product => product.Id != excludingId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public Task<bool> ActiveCategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return _dbContext.TblCategories
                .AsNoTracking()
                .AnyAsync(category => category.Id == categoryId && category.IsActive == 1, cancellationToken);
        }

        public async Task AddAsync(TblProduct product, CancellationToken cancellationToken = default)
        {
            await _dbContext.TblProducts.AddAsync(product, cancellationToken);
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
}
