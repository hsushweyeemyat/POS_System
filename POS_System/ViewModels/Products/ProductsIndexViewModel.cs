using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Products;

public class ProductsIndexViewModel
{
    public ProductsListRequestViewModel Query { get; set; } = new();

    public CreateProductViewModel CreateProduct { get; set; } = new();

    public UpdateProductViewModel EditProduct { get; set; } = new();

    public IReadOnlyList<ProductCategoryOptionViewModel> AvailableCategories { get; set; } = Array.Empty<ProductCategoryOptionViewModel>();

    public PagedResult<ProductListItemViewModel> ProductsPage { get; set; } = PagedResult<ProductListItemViewModel>.Empty();

    public string ActiveModal { get; set; } = string.Empty;
}
