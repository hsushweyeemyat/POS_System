using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.ViewModels.Shared;

namespace POS_System.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Sales");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Policy = "AdminOnly")]
    public IActionResult Admin()
    {
        return View();
    }

    [Authorize(Policy = "SalesAccess")]
    public IActionResult Sales()
    {
        return RedirectToAction("Index", "Sales");
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

