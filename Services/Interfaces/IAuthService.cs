using SecureTaskApi.DTOs;

namespace SecureTaskApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}