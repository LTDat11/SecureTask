using Microsoft.AspNetCore.Mvc;
using SecureTaskWeb.Middlewares;
using SecureTaskWeb.Models;

namespace SecureTaskWeb.Controllers;

/// <summary>
/// Main controller for the application dashboard
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the main dashboard (requires authentication)
    /// </summary>
    public IActionResult Index()
    {
        // Check if user is authenticated
        var userSession = HttpContext.Session.Get<UserSession>("UserSession");

        if (userSession == null)
        {
            _logger.LogWarning("Unauthorized access attempt to dashboard");
            return RedirectToAction("Index", "Auth");
        }

        _logger.LogInformation("User {Username} accessed dashboard", userSession.Username);
        ViewData["Username"] = userSession.Username;
        ViewData["Role"] = userSession.Role;
        ViewData["TokenExpiry"] = userSession.TokenExpiry;

        return View();
    }

    /// <summary>
    /// Displays error page
    /// </summary>
    public IActionResult Error()
    {
        var requestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var model = new ErrorViewModel { RequestId = requestId };
        return View(model);
    }
}
