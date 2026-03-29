using POS_System.ViewModels.Shared;

namespace POS_System.ViewModels.Products;

public class ProductsListRequestViewModel : PaginationRequest
{
    private string _searchTerm = string.Empty;

    public string SearchTerm
    {
        get => _searchTerm;
        set => _searchTerm = value?.Trim() ?? string.Empty;
    }
}
