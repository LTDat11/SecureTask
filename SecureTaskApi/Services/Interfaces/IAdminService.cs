using SecureTaskApi.DTOs;

namespace SecureTaskApi.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> CreateUserAsync(AdminCreateUserRequest request);
    Task SetActiveAsync(Guid userId, bool isActive);
    Task AdminChangePasswordAsync(Guid userId, AdminChangePasswordRequest request);
}
