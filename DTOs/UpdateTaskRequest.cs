using SecureTaskApi.Entities;
namespace SecureTaskApi.DTOs;

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }

    public Entities.TaskStatus Status { get; set; }
}