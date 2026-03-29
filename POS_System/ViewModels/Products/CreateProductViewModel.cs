using System.ComponentModel.DataAnnotations;

namespace POS_System.ViewModels.Products;

public class CreateProductViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Price must be zero or greater.")]
    public int Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be zero or greater.")]
    [Display(Name = "Stock quantity")]
    public int StockQty { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Active product")]
    public bool IsActive { get; set; } = true;
}
