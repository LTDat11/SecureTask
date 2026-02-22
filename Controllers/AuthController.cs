using Microsoft.AspNetCore.Mvc;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SecureTaskApi.Exceptions;


namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var exits = await _context.Users
            .AnyAsync(u => u.UserName == request.Username);

        if (exits)
            throw new BadRequestException("Username already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.Username,
            PasswordHash = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse { UserName = user.UserName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user == null)
            throw new NotFoundException("User not found");

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
            throw new BadRequestException("Invalid password");

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "SecureTaskApi";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "SecureTaskApiUser";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new AuthResponse { UserName = user.UserName, Token = jwt });
    }
}