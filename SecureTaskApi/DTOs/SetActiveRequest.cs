using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.DTOs;

public class SetActiveRequest
{
    [Required]
    public bool IsActive { get; set; }
}
