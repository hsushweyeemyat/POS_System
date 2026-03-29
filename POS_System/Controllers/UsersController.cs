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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _userManagementService.BuildIndexViewModelAsync(cancellationToken: cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsersIndexViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidViewModel = await _userManagementService.BuildIndexViewModelAsync(model.CreateUser, cancellationToken);
            return View("Index", invalidViewModel);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserId, out var createdByUserId))
        {
            return Forbid();
        }

        var result = await _userManagementService.CreateUserAsync(model.CreateUser, createdByUserId, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var failedViewModel = await _userManagementService.BuildIndexViewModelAsync(model.CreateUser, cancellationToken);
            return View("Index", failedViewModel);
        }

        TempData["StatusMessage"] = "User registered successfully.";
        return RedirectToAction(nameof(Index));
    }
}
