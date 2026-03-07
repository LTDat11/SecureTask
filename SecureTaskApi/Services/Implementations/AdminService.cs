using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;

    public AdminService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => MapToResponse(u));
    }

    public async Task<UserResponse> CreateUserAsync(AdminCreateUserRequest request)
    {
        if (request.Role != UserRoles.Admin && request.Role != UserRoles.User)
            throw new BadRequestException($"Invalid role. Must be '{UserRoles.Admin}' or '{UserRoles.User}'.");

        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (exists)
            throw new BadRequestException("Username already exists");

        var user = new User
        {
            UserName = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        user.IsActive = isActive;
        await _userRepository.SaveChangesAsync();
    }

    public async Task AdminChangePasswordAsync(Guid userId, AdminChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.SaveChangesAsync();
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Role = u.Role,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
