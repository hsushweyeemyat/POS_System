using Microsoft.AspNetCore.Identity;
using POS_System.Data.Entities;
using POS_System.Repositories.Interfaces;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Users;

namespace POS_System.Services.Implementations;

public class UserManagementService : IUserManagementService
{
    private static readonly string[] AllowedRoles = ["Admin", "Cashier"];

    private readonly IUserManagementRepository _userManagementRepository;
    private readonly UserManager<TblUser> _userManager;

    public UserManagementService(
        IUserManagementRepository userManagementRepository,
        UserManager<TblUser> userManager)
    {
        _userManagementRepository = userManagementRepository;
        _userManager = userManager;
    }

    public async Task<UsersIndexViewModel> BuildIndexViewModelAsync(
        CreateUserViewModel? createUser = null,
        CancellationToken cancellationToken = default)
    {
        var users = await _userManagementRepository.GetAllAsync(cancellationToken);

        return new UsersIndexViewModel
        {
            CreateUser = createUser ?? new CreateUserViewModel(),
            AvailableRoles = AllowedRoles,
            Users = users
                .Select(user => new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive == 1,
                    CreatedDate = user.CreatedDate
                })
                .ToList()
        };
    }

    public async Task<IdentityResult> CreateUserAsync(
        CreateUserViewModel model,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!AllowedRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Role must be Admin or Cashier."
            });
        }

        var user = new TblUser
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Role = AllowedRoles.First(role => role.Equals(model.Role, StringComparison.OrdinalIgnoreCase)),
            IsActive = model.IsActive ? (byte)1 : (byte)0,
            CreatedBy = createdByUserId,
            CreatedDate = DateTime.UtcNow
        };

        return await _userManager.CreateAsync(user, model.Password);
    }
}
