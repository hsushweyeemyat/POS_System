using Microsoft.EntityFrameworkCore.Storage;
using POS_System.Data.Entities;

namespace POS_System.Repositories.Interfaces;

public interface ISalesRepository
{
    Task<IReadOnlyList<TblProduct>> SearchSellableProductsAsync(string? searchTerm, CancellationToken cancellationToken = default);

    Task<TblProduct?> GetActiveProductByIdAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TblProduct>> GetActiveProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TblProduct>> GetTrackedProductsByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    Task<bool> InvoiceExistsAsync(string invoiceNo, CancellationToken cancellationToken = default);

    Task AddSaleAsync(TblSale sale, CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TblSale?> GetSaleByIdAsync(long saleId, int userId, CancellationToken cancellationToken = default);
}
