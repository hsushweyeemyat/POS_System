using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Users;

public class UsersIndexViewModel
{
    public UsersListRequestViewModel Query { get; set; } = new();

    public CreateUserViewModel CreateUser { get; set; } = new();

    public UpdateUserViewModel EditUser { get; set; } = new();

    public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();

    public PagedResult<UserListItemViewModel> UsersPage { get; set; } = PagedResult<UserListItemViewModel>.Empty();

    public string ActiveModal { get; set; } = string.Empty;
}
