using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = default!;
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = default!;
}