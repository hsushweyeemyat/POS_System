using POS_System.Data.Entities;
using POS_System.ViewModels.Shared;

namespace POS_System.Repositories.Interfaces;

public interface IUserManagementRepository
{
    Task<PagedResult<TblUser>> GetPagedAsync(
        string? searchTerm,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<TblUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
