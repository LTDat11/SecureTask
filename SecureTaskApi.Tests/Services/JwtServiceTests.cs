using Xunit;
using SecureTaskApi.Entities;
using SecureTaskApi.Services.Implementations;
using System.IdentityModel.Tokens.Jwt;

namespace SecureTaskApi.Tests.Services;

public class JwtServiceTests : IDisposable
{
    private const string TestJwtKey = "super-secret-test-key-that-is-at-least-32-chars";

    public JwtServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", TestJwtKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", null);
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        var jwtService = new JwtService();
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };

        var token = jwtService.GenerateToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ValidUser_TokenContainsUserClaims()
    {
        var jwtService = new JwtService();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "alice" };

        var token = jwtService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        var nameClaim = parsed.Claims.FirstOrDefault(c => c.Type == "unique_name");
        var subClaim = parsed.Claims.FirstOrDefault(c =>
            c.Type == "nameid" || c.Type == "sub" ||
            c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

        Assert.True(
            nameClaim?.Value == "alice" || parsed.Claims.Any(c => c.Value == "alice"),
            "Token should contain the username");
        Assert.True(
            parsed.Claims.Any(c => c.Value == userId.ToString()),
            "Token should contain the user ID");
    }

    [Fact]
    public void GenerateToken_JwtKeyNotConfigured_ThrowsException()
    {
        Environment.SetEnvironmentVariable("JWT_KEY", null);

        var jwtService = new JwtService();
        var user = new User { Id = Guid.NewGuid(), UserName = "alice" };

        Assert.Throws<Exception>(() => jwtService.GenerateToken(user));
    }
}
