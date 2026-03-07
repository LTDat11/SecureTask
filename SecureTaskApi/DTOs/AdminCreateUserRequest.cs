using System.ComponentModel.DataAnnotations;
using SecureTaskApi.Entities;

namespace SecureTaskApi.DTOs;

public class AdminCreateUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    [Required]
    public string Role { get; set; } = UserRoles.User;
}
