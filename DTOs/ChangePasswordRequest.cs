using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.DTOs;

public class ChangePasswordRequest
{
    [Required]
    [MinLength(6)]
    public string CurrentPassword { get; set; } = null!;
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
    [Required]
    [MinLength(6)]
    public string ConfirmNewPassword { get; set; } = null!;
}