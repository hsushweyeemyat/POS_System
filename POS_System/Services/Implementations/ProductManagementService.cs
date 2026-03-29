using Microsoft.AspNetCore.Identity;
using POS_System.Data.Entities;
using POS_System.Repositories.Interfaces;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Products;
using POS_System.ViewModels.Shared;

namespace POS_System.Services.Implementations;

public class ProductManagementService : IProductManagementService
{
    private readonly IProductManagementRepository _productManagementRepository;

    public ProductManagementService(IProductManagementRepository productManagementRepository)
    {
        _productManagementRepository = productManagementRepository;
    }

    public async Task<ProductsIndexViewModel> BuildIndexViewModelAsync(
        ProductsListRequestViewModel? query = null,
        CreateProductViewModel? createProduct = null,
        UpdateProductViewModel? editProduct = null,
        string? activeModal = null,
        CancellationToken cancellationToken = default)
    {
        var listQuery = query ?? new ProductsListRequestViewModel();
        var productsPage = await _productManagementRepository.GetPagedAsync(
            listQuery.SearchTerm,
            listQuery,
            cancellationToken);
        var categories = await _productManagementRepository.GetActiveCategoriesAsync(cancellationToken);

        return new ProductsIndexViewModel
        {
            Query = new ProductsListRequestViewModel
            {
                SearchTerm = listQuery.SearchTerm,
                Page = productsPage.Page,
                PageSize = productsPage.PageSize
            },
            CreateProduct = createProduct ?? new CreateProductViewModel(),
            EditProduct = editProduct ?? new UpdateProductViewModel(),
            AvailableCategories = categories
                .Select(category => new ProductCategoryOptionViewModel
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToList(),
            ActiveModal = activeModal ?? string.Empty,
            ProductsPage = new PagedResult<ProductListItemViewModel>(
                productsPage.Items
                    .Select(product => new ProductListItemViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        CategoryName = product.Category?.Name ?? string.Empty,
                        Price = product.Price,
                        StockQty = product.StockQty,
                        IsActive = product.IsActive == 1,
                        CreatedDate = product.CreatedDate
                    })
                    .ToList(),
                productsPage.Page,
                productsPage.PageSize,
                productsPage.TotalCount)
        };
    }

    public async Task<UpdateProductViewModel?> GetProductForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productManagementRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new UpdateProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            StockQty = product.StockQty,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive == 1
        };
    }

    public async Task<IdentityResult> CreateProductAsync(
        CreateProductViewModel model,
        int createdByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var productName = model.Name.Trim();

        if (await _productManagementRepository.ExistsByNameAsync(productName, cancellationToken: cancellationToken))
        {
            return Failed("Product name already exists.");
        }

        if (!await _productManagementRepository.ActiveCategoryExistsAsync(model.CategoryId, cancellationToken))
        {
            return Failed("Selected category was not found.");
        }

        var product = new TblProduct
        {
            Name = productName,
            Price = model.Price,
            StockQty = model.StockQty,
            CategoryId = model.CategoryId,
            IsActive = 1,
            CreatedBy = createdByUserId,
            CreatedDate = DateTime.UtcNow
        };

        await _productManagementRepository.AddAsync(product, cancellationToken);
        await _productManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateProductAsync(
        UpdateProductViewModel model,
        int modifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = await _productManagementRepository.GetTrackedByIdAsync(model.Id, cancellationToken);

        if (product is null || product.IsActive == 0)
        {
            return Failed("Product was not found.");
        }

        var productName = model.Name.Trim();

        if (await _productManagementRepository.ExistsByNameAsync(
                productName,
                model.Id,
                cancellationToken))
        {
            return Failed("Product name already exists.");
        }

        if (!await _productManagementRepository.ActiveCategoryExistsAsync(model.CategoryId, cancellationToken))
        {
            return Failed("Selected category was not found.");
        }

        product.Name = productName;
        product.Price = model.Price;
        product.StockQty = model.StockQty;
        product.CategoryId = model.CategoryId;
        product.IsActive = 1;
        product.ModifiedBy = modifiedByUserId;
        product.ModifiedDate = DateTime.UtcNow;

        await _productManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteProductAsync(
        int id,
        int modifiedByUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = await _productManagementRepository.GetTrackedByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return Failed("Product was not found.");
        }

        if (product.IsActive == 0)
        {
            return Failed("Product is already inactive.");
        }

        product.IsActive = 0;
        product.ModifiedBy = modifiedByUserId;
        product.ModifiedDate = DateTime.UtcNow;

        await _productManagementRepository.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    private static IdentityResult Failed(string description)
        => IdentityResult.Failed(new IdentityError
        {
            Description = description
        });
}
