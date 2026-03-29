using Microsoft.AspNetCore.Identity;
using POS_System.ViewModels.Categories;

namespace POS_System.Services.Interfaces;

public interface ICategoryManagementService
{
    Task<CategoriesIndexViewModel> BuildIndexViewModelAsync(
        CategoriesListRequestViewModel? query = null,
        CreateCategoryViewModel? createCategory = null,
        UpdateCategoryViewModel? editCategory = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateCategoryAsync(
        CreateCategoryViewModel model,
        int createdByUserId,
        CancellationToken cancellationToken = default);

    Task<UpdateCategoryViewModel?> GetCategoryForEditAsync(int id, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateCategoryAsync(
        UpdateCategoryViewModel model,
        int modifiedByUserId,
        CancellationToken cancellationToken = default);

    Task<IdentityResult> DeleteCategoryAsync(
        int id,
        int modifiedByUserId,
        CancellationToken cancellationToken = default);
}
