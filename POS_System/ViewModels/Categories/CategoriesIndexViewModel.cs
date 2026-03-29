using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Categories;

public class CategoriesIndexViewModel
{
    public CategoriesListRequestViewModel Query { get; set; } = new();

    public CreateCategoryViewModel CreateCategory { get; set; } = new();

    public UpdateCategoryViewModel EditCategory { get; set; } = new();

    public PagedResult<CategoryListItemViewModel> CategoriesPage { get; set; } = PagedResult<CategoryListItemViewModel>.Empty();

    public string ActiveModal { get; set; } = string.Empty;
}
