namespace SecureTaskApi.DTOs;

public class CreateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }

}
