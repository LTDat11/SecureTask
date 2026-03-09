using Microsoft.AspNetCore.Mvc;
using SecureTaskWeb.Common;
using SecureTaskWeb.Helpers;
using SecureTaskWeb.Middlewares;
using SecureTaskWeb.Models;
using SecureTaskWeb.Models.DTOs;
using SecureTaskWeb.Services;
using SecureTaskWeb.Services.Interfaces;

namespace SecureTaskWeb.Controllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the login/register page or dashboard if user is logged in
    /// </summary>
    public IActionResult Index()
    {
        // Check if user is already logged in
        var userSession = HttpContext.Session.Get<UserSession>("UserSession");

        if (userSession != null)
        {
            // User is logged in, show dashboard
            return View("~/Views/Home/Index.cshtml");
        }

        // User is not logged in, show login page
        return View("~/Views/Auth/Login.cshtml");
    }

    /// <summary>
    /// Displays the login page (GET request)
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View("~/Views/Auth/Login.cshtml");
    }

    /// <summary>
    /// Handles login form submission (AJAX)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(model);

        if (!result.Success)
        {
            _logger.LogWarning("Login attempt failed for user {Username}", model.Username);
            return BadRequest(new { error = result.Error ?? "Sai tài khoản hoặc mật khẩu" });
        }

        // Get token expiry time
        var expiryTime = JwtHelper.GetTokenExpiry(result.Data?.Token ?? string.Empty);

        // Store user session
        var userSession = new UserSession
        {
            Username = model.Username,
            Token = result.Data?.Token ?? string.Empty,
            TokenExpiry = expiryTime ?? DateTime.UtcNow.AddDays(7),
            Role = result.Data?.Role ?? AppConstants.DefaultRole,
            LoginTime = DateTime.UtcNow
        };

        HttpContext.Session.Set("UserSession", userSession);

        _logger.LogInformation("User {Username} logged in successfully. Token expires at {Expiry}",
            model.Username, userSession.TokenExpiry);

        return Ok(new { success = true, message = "Đăng nhập thành công" });
    }

    /// <summary>
    /// Handles registration form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(model);

        if (!result.Success)
        {
            _logger.LogWarning("Registration attempt failed for user {Username}", model.Username);
            return BadRequest(new { error = result.Error ?? "Đăng ký thất bại" });
        }

        _logger.LogInformation("User {Username} registered successfully", model.Username);

        return Ok(new { success = true, message = "Đăng ký thành công. Vui lòng đăng nhập!" });
    }

    /// <summary>
    /// Logs out the user by removing the session
    /// </summary>
    [HttpGet]
    public IActionResult Logout()
    {
        var userSession = HttpContext.Session.Get<UserSession>("UserSession");

        if (userSession != null)
        {
            _logger.LogInformation("User {Username} logged out", userSession.Username);
        }

        HttpContext.Session.Remove("UserSession");
        HttpContext.Session.Clear();

        TempData["Toast"] = "Đã đăng xuất";
        return RedirectToAction("Index");
    }
}
