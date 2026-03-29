using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.Services.Interfaces;
using POS_System.ViewModels.Sales;

namespace POS_System.Controllers;

[Authorize(Policy = "SalesAccess")]
public class SalesController : Controller
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var viewModel = await _salesService.BuildIndexViewModelAsync(userId, searchTerm, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> State(string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var state = await _salesService.BuildIndexViewModelAsync(userId, searchTerm, cancellationToken);
        return Json(state);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity, string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var response = await _salesService.AddToCartAsync(userId, productId, quantity, searchTerm, cancellationToken);
        return Json(response);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity, string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var response = await _salesService.UpdateCartItemAsync(userId, productId, quantity, searchTerm, cancellationToken);
        return Json(response);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int productId, string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var response = await _salesService.RemoveCartItemAsync(userId, productId, searchTerm, cancellationToken);
        return Json(response);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(string? searchTerm, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var response = await _salesService.CheckoutAsync(userId, searchTerm, cancellationToken);

        if (response.Succeeded)
        {
            response.RedirectUrl = Url.Action(nameof(Invoice), new { id = response.SaleId }) ?? string.Empty;
        }

        return Json(response);
    }

    [HttpGet]
    public async Task<IActionResult> Invoice(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var viewModel = await _salesService.GetInvoiceAsync(
            id,
            userId,
            User.IsInRole("Admin"),
            cancellationToken);

        if (viewModel is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> History(
        [FromQuery] SalesHistoryListRequestViewModel request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Forbid();
        }

        var viewModel = await _salesService.BuildHistoryViewModelAsync(
            userId,
            User.IsInRole("Admin"),
            request,
            cancellationToken);

        return View(viewModel);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out userId);
    }
}
