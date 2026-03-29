using System.ComponentModel.DataAnnotations;

namespace POS_System.ViewModels.Users;

public class UpdateUserViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Cashier";

    [Display(Name = "Active user")]
    public bool IsActive { get; set; } = true;
}
