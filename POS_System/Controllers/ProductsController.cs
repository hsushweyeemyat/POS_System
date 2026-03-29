using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Products;

namespace POS_System.Controllers;

[Authorize(Policy = "AdminOnly")]
public class ProductsController : Controller
{
    private readonly IProductManagementService _productManagementService;

    public ProductsController(IProductManagementService productManagementService)
    {
        _productManagementService = productManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductsListRequestViewModel query, CancellationToken cancellationToken)
    {
        var viewModel = await _productManagementService.BuildIndexViewModelAsync(query, cancellationToken: cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProductsListRequestViewModel query,
        [Bind(Prefix = nameof(ProductsIndexViewModel.CreateProduct))] CreateProductViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _productManagementService.BuildIndexViewModelAsync(
                query,
                createProduct: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var createdByUserId))
        {
            return Forbid();
        }

        var result = await _productManagementService.CreateProductAsync(model, createdByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _productManagementService.BuildIndexViewModelAsync(
                query,
                createProduct: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "Product created successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productManagementService.GetProductForEditAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Json(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        ProductsListRequestViewModel query,
        [Bind(Prefix = nameof(ProductsIndexViewModel.EditProduct))] UpdateProductViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _productManagementService.BuildIndexViewModelAsync(
                query,
                editProduct: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _productManagementService.UpdateProductAsync(model, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _productManagementService.BuildIndexViewModelAsync(
                query,
                editProduct: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "Product updated successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, ProductsListRequestViewModel query, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _productManagementService.DeleteProductAsync(id, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = result.Errors.FirstOrDefault()?.Description ?? "Unable to delete product.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
        }

        TempData["StatusMessage"] = "Product deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out userId);
    }

    private static object BuildIndexRouteValues(ProductsListRequestViewModel query) => new
    {
        page = query.Page,
        pageSize = query.PageSize,
        searchTerm = query.SearchTerm
    };
}
