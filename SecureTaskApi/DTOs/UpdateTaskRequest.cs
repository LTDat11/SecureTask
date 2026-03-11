using System.ComponentModel.DataAnnotations;
using SecureTaskApi.Entities;
namespace SecureTaskApi.DTOs;

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }

    public Entities.TaskStatus Status { get; set; }

    public Entities.PriorityStatus Priority  { get; set; }
}