using System.ComponentModel.DataAnnotations;
using Microsoft.Net.Http.Headers;

namespace SecureTaskApi.Entities;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = default!;

    public PriorityStatus Priority { get; set; } = default!;

    public DateTime? Deadline { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}