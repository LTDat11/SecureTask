using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
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
}