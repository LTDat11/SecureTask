using Moq;
using Xunit;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Implementations;

namespace SecureTaskApi.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _adminService = new AdminService(_userRepoMock.Object);
    }

    // ── GetAllUsersAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), UserName = "alice", Role = UserRoles.User,  IsActive = true  },
            new() { Id = Guid.NewGuid(), UserName = "bob",   Role = UserRoles.Admin, IsActive = false }
        };

        _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = (await _adminService.GetAllUsersAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.UserName == "alice" && u.Role == UserRoles.User && u.IsActive);
        Assert.Contains(result, u => u.UserName == "bob" && u.Role == UserRoles.Admin && !u.IsActive);
    }

    // ── CreateUserAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUserAsync_UsernameAlreadyExists_ThrowsBadRequestException()
    {
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync("existing")).ReturnsAsync(true);

        var request = new AdminCreateUserRequest { Username = "existing", Password = "pass123", Role = UserRoles.User };

        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_InvalidRole_ThrowsBadRequestException()
    {
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>())).ReturnsAsync(false);

        var request = new AdminCreateUserRequest { Username = "newuser", Password = "pass123", Role = "SuperAdmin" };

        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_ValidUserRole_ReturnsUserResponse()
    {
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync("newuser")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new AdminCreateUserRequest { Username = "newuser", Password = "pass123", Role = UserRoles.User };

        var result = await _adminService.CreateUserAsync(request);

        Assert.Equal("newuser", result.UserName);
        Assert.Equal(UserRoles.User, result.Role);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateUserAsync_ValidAdminRole_ReturnsAdminResponse()
    {
        _userRepoMock.Setup(r => r.ExistsByUsernameAsync("newadmin")).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new AdminCreateUserRequest { Username = "newadmin", Password = "pass123", Role = UserRoles.Admin };

        var result = await _adminService.CreateUserAsync(request);

        Assert.Equal(UserRoles.Admin, result.Role);
    }

    // ── SetActiveAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SetActiveAsync_UserNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _adminService.SetActiveAsync(id, false));
    }

    [Fact]
    public async Task SetActiveAsync_DeactivatesUser()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "alice", IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _adminService.SetActiveAsync(user.Id, false);

        Assert.False(user.IsActive);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SetActiveAsync_ActivatesUser()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = "alice", IsActive = false };
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _adminService.SetActiveAsync(user.Id, true);

        Assert.True(user.IsActive);
    }

    // ── AdminChangePasswordAsync ───────────────────────────────────────────

    [Fact]
    public async Task AdminChangePasswordAsync_UserNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _adminService.AdminChangePasswordAsync(id, new AdminChangePasswordRequest { NewPassword = "new123" }));
    }

    [Fact]
    public async Task AdminChangePasswordAsync_ValidRequest_UpdatesPasswordHash()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("old-password")
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _adminService.AdminChangePasswordAsync(user.Id, new AdminChangePasswordRequest { NewPassword = "new-password" });

        Assert.True(BCrypt.Net.BCrypt.Verify("new-password", user.PasswordHash));
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
