using Microsoft.AspNetCore.Identity;
using POS_System.Data.Entities;
using POS_System.Repositories.Interfaces;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Shared;
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
        UsersListRequestViewModel? query = null,
        CreateUserViewModel? createUser = null,
        UpdateUserViewModel? editUser = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default)
    {
        var listQuery = query ?? new UsersListRequestViewModel();
        var usersPage = await _userManagementRepository.GetPagedAsync(
            listQuery.SearchTerm,
            listQuery,
            cancellationToken);

        return new UsersIndexViewModel
        {
            Query = new UsersListRequestViewModel
            {
                SearchTerm = listQuery.SearchTerm,
                Page = usersPage.Page,
                PageSize = usersPage.PageSize
            },
            CreateUser = createUser ?? new CreateUserViewModel(),
            EditUser = editUser ?? new UpdateUserViewModel(),
            AvailableRoles = AllowedRoles,
            ActiveModal = activeModal ?? string.Empty,
            UsersPage = new PagedResult<UserListItemViewModel>(
                usersPage.Items
                    .Select(user => new UserListItemViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive == 1,
                        CreatedDate = user.CreatedDate
                    })
                    .ToList(),
                usersPage.Page,
                usersPage.PageSize,
                usersPage.TotalCount)
        };
    }

    public async Task<UpdateUserViewModel?> GetUserForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userManagementRepository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UpdateUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive == 1
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
            IsActive = 1,
            CreatedBy = createdByUserId,
            CreatedDate = DateTime.UtcNow
        };

        return await _userManager.CreateAsync(user, model.Password);
    }

    public async Task<IdentityResult> UpdateUserAsync(
        UpdateUserViewModel model,
        int modifiedByUserId,
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

        var user = await _userManager.FindByIdAsync(model.Id.ToString());

        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User was not found."
            });
        }

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.Role = AllowedRoles.First(role => role.Equals(model.Role, StringComparison.OrdinalIgnoreCase));
        user.IsActive = 1;
        user.ModifiedBy = modifiedByUserId;
        user.ModifiedDate = DateTime.UtcNow;

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserAsync(
        int id,
        int modifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User was not found."
            });
        }

        if (user.IsActive == 0)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User is already inactive."
            });
        }

        user.IsActive = 0;
        user.ModifiedBy = modifiedByUserId;
        user.ModifiedDate = DateTime.UtcNow;

        return await _userManager.UpdateAsync(user);
    }
}
