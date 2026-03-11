using System.ComponentModel.DataAnnotations;
using SecureTaskApi.Entities;

namespace SecureTaskApi.DTOs;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = null!;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    [Required(ErrorMessage = "This is required.")]
    public PriorityStatus Priority { get; set; }
}
