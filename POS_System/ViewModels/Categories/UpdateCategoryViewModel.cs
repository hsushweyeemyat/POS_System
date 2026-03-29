using System.ComponentModel.DataAnnotations;

namespace POS_System.ViewModels.Categories;

public class UpdateCategoryViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Active category")]
    public bool IsActive { get; set; } = true;
}
