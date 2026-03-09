using SecureTaskWeb.Common;
using SecureTaskWeb.Models.DTOs;
using SecureTaskWeb.Models.Responses;
using SecureTaskWeb.Services.Interfaces;

namespace SecureTaskWeb.Services;

/// <summary>
/// Service for handling authentication operations with the backend API
/// </summary>
public class AuthApiService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<AuthApiService> _logger;

    public AuthApiService(IApiClient apiClient, ILogger<AuthApiService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting login for user: {Username}", request.Username);

        var result = await _apiClient.PostAsync<LoginResponse>(ApiEndpoints.LoginEndpoint, request);

        if (!result.Success)
        {
            _logger.LogWarning("Login failed for user {Username}: {Error}", request.Username, result.Error);
        }
        else
        {
            _logger.LogInformation("Login successful for user: {Username}", request.Username);
        }

        return result;
    }

    public async Task<ApiResult<object>> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting registration for user: {Username}", request.Username);

        var result = await _apiClient.PostAsync<object>(ApiEndpoints.RegisterEndpoint, request);

        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for user {Username}: {Error}", request.Username, result.Error);
        }
        else
        {
            _logger.LogInformation("Registration successful for user: {Username}", request.Username);
        }

        return result;
    }
}
