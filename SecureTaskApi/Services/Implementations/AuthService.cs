using System.Security.Claims;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);

        if (exists)
            throw new BadRequestException("Username already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.Username,
            PasswordHash = hashedPassword
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse
        {
            UserName = user.UserName
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.UserName);

        if (user == null)
            throw new NotFoundException("User not found");

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
            throw new BadRequestException("Invalid password");

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            UserName = user.UserName,
            Token = token
        };
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User not found");

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
            throw new BadRequestException("Current password is incorrect");

        if (request.NewPassword == request.CurrentPassword)
            throw new BadRequestException("New password must be different from the current password");

        if (request.NewPassword != request.ConfirmNewPassword)
            throw new BadRequestException("New password and confirmation do not match");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.SaveChangesAsync();

        return new ChangePasswordResponse
        {
            Message = "Password changed successfully"
        };
    }
}