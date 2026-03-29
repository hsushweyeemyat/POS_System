namespace POS_System.ViewModels.Users;

public class UsersIndexViewModel
{
    public CreateUserViewModel CreateUser { get; set; } = new();

    public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();

    public IReadOnlyList<UserListItemViewModel> Users { get; set; } = Array.Empty<UserListItemViewModel>();
}
