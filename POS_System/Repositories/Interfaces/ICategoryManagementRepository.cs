using POS_System.Data.Entities;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Interfaces;

public interface ICategoryManagementRepository
{
    Task<PagedResult<TblCategory>> GetPagedAsync(
        string? searchTerm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<TblCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TblCategory?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, int? excludingId = null, CancellationToken cancellationToken = default);

    Task<bool> HasActiveProductsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task AddAsync(TblCategory category, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
