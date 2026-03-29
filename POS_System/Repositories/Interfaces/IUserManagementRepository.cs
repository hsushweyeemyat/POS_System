using POS_System.Data.Entities;

namespace POS_System.Repositories.Interfaces;

public interface IUserManagementRepository
{
    Task<IReadOnlyList<TblUser>> GetAllAsync(CancellationToken cancellationToken = default);
}
