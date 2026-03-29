using Microsoft.AspNetCore.Identity;
using POS_System.Data.Entities;
using POS_System.Repositories.Interfaces;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Categories;
using POS_System.ViewModels.Shared;

namespace POS_System.Services.Implementations;

public class CategoryManagementService : ICategoryManagementService
{
    private readonly ICategoryManagementRepository _categoryManagementRepository;

    public CategoryManagementService(ICategoryManagementRepository categoryManagementRepository)
    {
        _categoryManagementRepository = categoryManagementRepository;
    }

    public async Task<CategoriesIndexViewModel> BuildIndexViewModelAsync(
        CategoriesListRequestViewModel? query = null,
        CreateCategoryViewModel? createCategory = null,
        UpdateCategoryViewModel? editCategory = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default)
    {
        var listQuery = query ?? new CategoriesListRequestViewModel();
        var categoriesPage = await _categoryManagementRepository.GetPagedAsync(
            listQuery.SearchTerm,
            listQuery,
            cancellationToken);

        return new CategoriesIndexViewModel
        {
            Query = new CategoriesListRequestViewModel
            {
                SearchTerm = listQuery.SearchTerm,
                Page = categoriesPage.Page,
                PageSize = categoriesPage.PageSize
            },
            CreateCategory = createCategory ?? new CreateCategoryViewModel(),
            EditCategory = editCategory ?? new UpdateCategoryViewModel(),
            ActiveModal = activeModal ?? string.Empty,
            CategoriesPage = new PagedResult<CategoryListItemViewModel>(
                categoriesPage.Items
                    .Select(category => new CategoryListItemViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        IsActive = category.IsActive == 1,
                        CreatedDate = category.CreatedDate
                    })
                    .ToList(),
                categoriesPage.Page,
                categoriesPage.PageSize,
                categoriesPage.TotalCount)
        };
    }

    public async Task<UpdateCategoryViewModel?> GetCategoryForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryManagementRepository.GetByIdAsync(id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        return new UpdateCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            IsActive = category.IsActive == 1
        };
    }

    public async Task<IdentityResult> CreateCategoryAsync(
        CreateCategoryViewModel model,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var categoryName = model.Name.Trim();

        if (await _categoryManagementRepository.ExistsByNameAsync(categoryName, cancellationToken: cancellationToken))
        {
            return Failed("Category name already exists.");
        }

        var category = new TblCategory
        {
            Name = categoryName,
            IsActive = 1,
            CreatedBy = createdByUserId,
            CreatedDate = DateTime.UtcNow
        };

        await _categoryManagementRepository.AddAsync(category, cancellationToken);
        await _categoryManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateCategoryAsync(
        UpdateCategoryViewModel model,
        int modifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var category = await _categoryManagementRepository.GetTrackedByIdAsync(model.Id, cancellationToken);

        if (category is null || category.IsActive == 0)
        {
            return Failed("Category was not found.");
        }

        var categoryName = model.Name.Trim();

        if (await _categoryManagementRepository.ExistsByNameAsync(
                categoryName,
                model.Id,
                cancellationToken))
        {
            return Failed("Category name already exists.");
        }

        category.Name = categoryName;
        category.IsActive = 1;
        category.ModifiedBy = modifiedByUserId;
        category.ModifiedDate = DateTime.UtcNow;

        await _categoryManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteCategoryAsync(
        int id,
        int modifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var category = await _categoryManagementRepository.GetTrackedByIdAsync(id, cancellationToken);

        if (category is null)
        {
            return Failed("Category was not found.");
        }

        if (category.IsActive == 0)
        {
            return Failed("Category is already inactive.");
        }

        if (await _categoryManagementRepository.HasActiveProductsAsync(id, cancellationToken))
        {
            return Failed("Category cannot be deleted because it is assigned to active products.");
        }

        category.IsActive = 0;
        category.ModifiedBy = modifiedByUserId;
        category.ModifiedDate = DateTime.UtcNow;

        await _categoryManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    private static IdentityResult Failed(string description)
        => IdentityResult.Failed(new IdentityError
        {
            Description = description
        });
}
