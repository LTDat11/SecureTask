using Microsoft.AspNetCore.Mvc;
using SecureTaskApi.DTOs;
using SecureTaskApi.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // function to get user id from claims
    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userIdClaim) && Guid.TryParse(userIdClaim, out userId);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [EnableRateLimiting("LoginPolicy")] // Apply rate limiting to the login endpoint
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return StatusCode(401, ApiResponse<object>.Fail("Missing or invalid token."));

        if (!TryGetUserId(out var userId))
            return StatusCode(401, ApiResponse<object>.Fail("Invalid user claim in token."));

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse<string>.Ok("Password changed successfully"));
    }
}