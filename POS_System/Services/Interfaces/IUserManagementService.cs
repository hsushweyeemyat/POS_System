using Microsoft.AspNetCore.Identity;
using POS_System.ViewModels.Users;

namespace POS_System.Services.Interfaces;

public interface IUserManagementService
{
    Task<UsersIndexViewModel> BuildIndexViewModelAsync(
        UsersListRequestViewModel? query = null,
        CreateUserViewModel? createUser = null,
        UpdateUserViewModel? editUser = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateUserAsync(CreateUserViewModel model, int createdByUserId, CancellationToken cancellationToken = default);

    Task<UpdateUserViewModel?> GetUserForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateUserAsync(UpdateUserViewModel model, int modifiedByUserId, CancellationToken cancellationToken = default);

    Task<IdentityResult> DeleteUserAsync(int id, int modifiedByUserId, CancellationToken cancellationToken = default);
}
