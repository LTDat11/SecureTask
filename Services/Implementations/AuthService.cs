using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.UserName == request.Username);

        if (exists)
            throw new BadRequestException("Username already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.Username,
            PasswordHash = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserName = user.UserName
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

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