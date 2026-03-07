using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // GET api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserResponse>>.Ok(users));
    }

    // POST api/admin/users
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(AdminCreateUserRequest request)
    {
        var result = await _adminService.CreateUserAsync(request);
        return StatusCode(201, ApiResponse<UserResponse>.Ok(result));
    }

    // PUT api/admin/users/{id}/active
    [HttpPut("users/{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, SetActiveRequest request)
    {
        await _adminService.SetActiveAsync(id, request.IsActive);
        return Ok(ApiResponse<object>.Ok(null));
    }

    // PUT api/admin/users/{id}/password
    [HttpPut("users/{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, AdminChangePasswordRequest request)
    {
        await _adminService.AdminChangePasswordAsync(id, request);
        return Ok(ApiResponse<object>.Ok(null));
    }
}
