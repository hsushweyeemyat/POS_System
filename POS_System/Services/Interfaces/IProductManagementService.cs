using Microsoft.AspNetCore.Identity;
using POS_System.ViewModels.Products;

namespace POS_System.Services.Interfaces;

public interface IProductManagementService
{
    Task<ProductsIndexViewModel> BuildIndexViewModelAsync(
        ProductsListRequestViewModel? query = null,
        CreateProductViewModel? createProduct = null,
        UpdateProductViewModel? editProduct = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateProductAsync(
        CreateProductViewModel model,
        int createdByUserId,
        CancellationToken cancellationToken = default);

    Task<UpdateProductViewModel?> GetProductForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateProductAsync(
        UpdateProductViewModel model,
        int modifiedByUserId,
        CancellationToken cancellationToken = default);

    Task<IdentityResult> DeleteProductAsync(
        int id,
        int modifiedByUserId,
        CancellationToken cancellationToken = default);
}
