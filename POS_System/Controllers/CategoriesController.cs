using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Categories;

namespace POS_System.Controllers;

[Authorize(Policy = "AdminOnly")]
public class CategoriesController : Controller
{
    private readonly ICategoryManagementService _categoryManagementService;

    public CategoriesController(ICategoryManagementService categoryManagementService)
    {
        _categoryManagementService = categoryManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CategoriesListRequestViewModel query, CancellationToken cancellationToken)
    {
        var viewModel = await _categoryManagementService.BuildIndexViewModelAsync(query, cancellationToken: cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CategoriesListRequestViewModel query,
        [Bind(Prefix = nameof(CategoriesIndexViewModel.CreateCategory))] CreateCategoryViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _categoryManagementService.BuildIndexViewModelAsync(
                query,
                createCategory: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var createdByUserId))
        {
            return Forbid();
        }

        var result = await _categoryManagementService.CreateCategoryAsync(model, createdByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _categoryManagementService.BuildIndexViewModelAsync(
                query,
                createCategory: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "Category created successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryManagementService.GetCategoryForEditAsync(id, cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return Json(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        CategoriesListRequestViewModel query,
        [Bind(Prefix = nameof(CategoriesIndexViewModel.EditCategory))] UpdateCategoryViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _categoryManagementService.BuildIndexViewModelAsync(
                query,
                editCategory: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _categoryManagementService.UpdateCategoryAsync(model, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _categoryManagementService.BuildIndexViewModelAsync(
                query,
                editCategory: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "Category updated successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CategoriesListRequestViewModel query, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _categoryManagementService.DeleteCategoryAsync(id, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = result.Errors.FirstOrDefault()?.Description ?? "Unable to delete category.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
        }

        TempData["StatusMessage"] = "Category deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out userId);
    }

    private static object BuildIndexRouteValues(CategoriesListRequestViewModel query) => new
    {
        page = query.Page,
        pageSize = query.PageSize,
        searchTerm = query.SearchTerm
    };
}
