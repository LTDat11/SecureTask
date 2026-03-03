using Moq;
using Xunit;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Implementations;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _authService = new AuthService(_userRepoMock.Object, _jwtServiceMock.Object);
    }

    // ── RegisterAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_UsernameAlreadyExists_ThrowsBadRequestException()
    {
        _userRepoMock
            .Setup(r => r.ExistsByUsernameAsync("existinguser"))
            .ReturnsAsync(true);

        var request = new RegisterRequest { Username = "existinguser", Password = "password123" };

        await Assert.ThrowsAsync<BadRequestException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponseWithUsername()
    {
        _userRepoMock
            .Setup(r => r.ExistsByUsernameAsync("newuser"))
            .ReturnsAsync(false);
        _userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _userRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new RegisterRequest { Username = "newuser", Password = "password123" };

        var result = await _authService.RegisterAsync(request);

        Assert.Equal("newuser", result.UserName);
    }

    // ── LoginAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("ghost"))
            .ReturnsAsync((User?)null);

        var request = new LoginRequest { UserName = "ghost", Password = "password123" };

        await Assert.ThrowsAsync<NotFoundException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsBadRequestException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };

        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("alice"))
            .ReturnsAsync(user);

        var request = new LoginRequest { UserName = "alice", Password = "wrong-password" };

        await Assert.ThrowsAsync<BadRequestException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };

        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("alice"))
            .ReturnsAsync(user);
        _jwtServiceMock
            .Setup(j => j.GenerateToken(user))
            .Returns("mocked-jwt-token");

        var request = new LoginRequest { UserName = "alice", Password = "password123" };

        var result = await _authService.LoginAsync(request);

        Assert.Equal("alice", result.UserName);
        Assert.Equal("mocked-jwt-token", result.Token);
    }

    // ── ChangePasswordAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "old",
            NewPassword = "new123",
            ConfirmNewPassword = "new123"
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _authService.ChangePasswordAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task ChangePasswordAsync_CurrentPasswordIncorrect_ThrowsBadRequestException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password")
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrong-current",
            NewPassword = "new123",
            ConfirmNewPassword = "new123"
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.ChangePasswordAsync(user.Id, request));
    }

    [Fact]
    public async Task ChangePasswordAsync_SamePassword_ThrowsBadRequestException()
    {
        var password = "password123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = password,
            NewPassword = password,
            ConfirmNewPassword = password
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.ChangePasswordAsync(user.Id, request));
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordMismatch_ThrowsBadRequestException()
    {
        var password = "password123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = password,
            NewPassword = "new-password",
            ConfirmNewPassword = "different-password"
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _authService.ChangePasswordAsync(user.Id, request));
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_ReturnsSuccessMessage()
    {
        var password = "old-password";
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = password,
            NewPassword = "new-password",
            ConfirmNewPassword = "new-password"
        };

        var result = await _authService.ChangePasswordAsync(user.Id, request);

        Assert.Equal("Password changed successfully", result.Message);
    }
}
