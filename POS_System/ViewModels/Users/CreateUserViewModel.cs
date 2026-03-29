using System.ComponentModel.DataAnnotations;

namespace POS_System.ViewModels.Users;

public class CreateUserViewModel
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one lowercase letter and one digit.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirm password do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Cashier";

    [Display(Name = "Active user")]
    public bool IsActive { get; set; } = true;
}
