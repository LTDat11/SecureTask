using Microsoft.IdentityModel.Tokens;
using SecureTaskApi.Entities;
using SecureTaskApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureTaskApi.Services.Implementations;

public class JwtService : IJwtService
{
    public string GenerateToken(User user)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? throw new Exception("JWT_KEY not configured");

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
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}