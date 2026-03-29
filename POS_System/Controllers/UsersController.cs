using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Users;

namespace POS_System.Controllers;

//[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] UsersListRequestViewModel query, CancellationToken cancellationToken)
    {
        var viewModel = await _userManagementService.BuildIndexViewModelAsync(query, cancellationToken: cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UsersListRequestViewModel query,
        [Bind(Prefix = nameof(UsersIndexViewModel.CreateUser))] CreateUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _userManagementService.BuildIndexViewModelAsync(
                query,
                createUser: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var createdByUserId))
        {
            return Forbid();
        }

        var result = await _userManagementService.CreateUserAsync(model, createdByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _userManagementService.BuildIndexViewModelAsync(
                query,
                createUser: model,
                activeModal: "create",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "User registered successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserForEditAsync(id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Json(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        UsersListRequestViewModel query,
        [Bind(Prefix = nameof(UsersIndexViewModel.EditUser))] UpdateUserViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _userManagementService.BuildIndexViewModelAsync(
                query,
                editUser: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", invalidViewModel);
        }

        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _userManagementService.UpdateUserAsync(model, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _userManagementService.BuildIndexViewModelAsync(
                query,
                editUser: model,
                activeModal: "edit",
                cancellationToken: cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "User updated successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, UsersListRequestViewModel query, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var modifiedByUserId))
        {
            return Forbid();
        }

        var result = await _userManagementService.DeleteUserAsync(id, modifiedByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = result.Errors.FirstOrDefault()?.Description ?? "Unable to delete user.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
        }

        TempData["StatusMessage"] = "User deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index), BuildIndexRouteValues(query));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out userId);
    }

    private static object BuildIndexRouteValues(UsersListRequestViewModel query) => new
    {
        page = query.Page,
        pageSize = query.PageSize,
        searchTerm = query.SearchTerm
    };
}
