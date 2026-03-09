using SecureTaskWeb.Models.DTOs;
using SecureTaskWeb.Models.Responses;

namespace SecureTaskWeb.Services.Interfaces;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates user with username and password
    /// </summary>
    Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<ApiResult<object>> RegisterAsync(RegisterRequest request);
}
