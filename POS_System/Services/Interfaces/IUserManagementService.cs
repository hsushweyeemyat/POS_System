using Microsoft.AspNetCore.Identity;
using POS_System.ViewModels.Users;

namespace POS_System.Services.Interfaces;

public interface IUserManagementService
{
    Task<UsersIndexViewModel> BuildIndexViewModelAsync(CreateUserViewModel? createUser = null, CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateUserAsync(CreateUserViewModel model, int createdByUserId, CancellationToken cancellationToken = default);
}
