using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using POS_System.Data.Contexts;
using POS_System.Data.Entities;
using POS_System.Extensions;
using POS_System.Models.Sales;
using POS_System.Repositories.Interfaces;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Implementations;

public class SalesRepository : ISalesRepository
{
    private const int SearchTake = 24;
    private readonly ApplicationDbContext _dbContext;

    public SalesRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TblProduct>> SearchSellableProductsAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblProducts
            .AsNoTracking()
            .Where(product => product.IsActive == 1 && product.StockQty > 0);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";

            query = query.Where(product =>
                EF.Functions.Like(product.Name, pattern));
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
                Category = product.Category == null
                    ? null
                    : new TblCategory
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name
                    }
            })
            .Take(SearchTake)
            .ToListAsync(cancellationToken);
    }

    public async Task<TblProduct?> GetActiveProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TblProducts
            .AsNoTracking()
            .Where(product => product.Id == productId && product.IsActive == 1)
            .Select(product => new TblProduct
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQty = product.StockQty,
                CategoryId = product.CategoryId,
                Category = product.Category == null
                    ? null
                    : new TblCategory
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name
                    }
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TblProduct>> GetActiveProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
        {
            return Array.Empty<TblProduct>();
        }

        return await _dbContext.TblProducts
            .AsNoTracking()
            .Where(product => product.IsActive == 1 && productIds.Contains(product.Id))
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Select(product => new TblProduct
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQty = product.StockQty,
                CategoryId = product.CategoryId,
                Category = product.Category == null
                    ? null
                    : new TblCategory
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name
                    }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TblProduct>> GetTrackedProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
        {
            return Array.Empty<TblProduct>();
        }

        return await _dbContext.TblProducts
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> InvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        return _dbContext.TblSales
            .AsNoTracking()
            .AnyAsync(sale => sale.InvoiceNo == invoiceNo, cancellationToken);
    }

    public async Task AddSaleAsync(TblSale sale, CancellationToken cancellationToken = default)
    {
        await _dbContext.TblSales.AddAsync(sale, cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PagedResult<SaleHistoryRecord>> GetSaleHistoryAsync(
        int? userId,
        string? searchTerm,
        DateTime? startDateInclusive,
        DateTime? endDateExclusive,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblSales.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(sale => sale.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{EscapeLikePattern(searchTerm.Trim())}%";
            query = query.Where(sale => EF.Functions.Like(sale.InvoiceNo, pattern));
        }

        if (startDateInclusive.HasValue)
        {
            query = query.Where(sale => sale.SaleDate >= startDateInclusive.Value);
        }

        if (endDateExclusive.HasValue)
        {
            query = query.Where(sale => sale.SaleDate < endDateExclusive.Value);
        }

        return query
            .OrderByDescending(sale => sale.SaleDate)
            .ThenByDescending(sale => sale.Id)
            .Select(sale => new SaleHistoryRecord
            {
                SaleId = sale.Id,
                InvoiceNo = sale.InvoiceNo,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                UserId = sale.UserId,
                UserName = sale.User == null ? "Unknown cashier" : sale.User.FullName,
                LineItemCount = sale.SaleItems.Count(),
                TotalQuantity = sale.SaleItems.Sum(item => (int?)item.Qty) ?? 0
            })
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<TblSale?> GetSaleByIdAsync(long saleId, int? userId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TblSales
            .AsNoTracking()
            .Where(sale => sale.Id == saleId);

        if (userId.HasValue)
        {
            query = query.Where(sale => sale.UserId == userId.Value);
        }

        return await query
            .Select(sale => new TblSale
            {
                Id = sale.Id,
                InvoiceNo = sale.InvoiceNo,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                UserId = sale.UserId,
                User = sale.User == null
                    ? null
                    : new TblUser
                    {
                        Id = sale.User.Id,
                        FullName = sale.User.FullName,
                        Email = sale.User.Email
                    },
                SaleItems = sale.SaleItems
                    .OrderBy(item => item.Id)
                    .Select(item => new TblSaleItem
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        Price = item.Price,
                        Product = item.Product == null
                            ? null
                            : new TblProduct
                            {
                                Id = item.Product.Id,
                                Name = item.Product.Name
                            }
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);
}
