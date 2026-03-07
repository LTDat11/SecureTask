using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.DTOs;

public class AdminChangePasswordRequest
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = default!;
}
