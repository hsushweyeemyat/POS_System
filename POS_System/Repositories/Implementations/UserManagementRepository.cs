using Microsoft.EntityFrameworkCore;
using POS_System.Data.Contexts;
using POS_System.Data.Entities;
using POS_System.Repositories.Interfaces;

namespace POS_System.Repositories.Implementations;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserManagementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TblUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TblUsers
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);
    }

    
}
