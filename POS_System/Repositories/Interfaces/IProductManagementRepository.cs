using POS_System.Data.Entities;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Interfaces;

public interface IProductManagementRepository
{
    Task<PagedResult<TblProduct>> GetPagedAsync(
        string? searchTerm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<TblProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TblProduct?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TblCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, int? excludingId = null, CancellationToken cancellationToken = default);

    Task<bool> ActiveCategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task AddAsync(TblProduct product, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
