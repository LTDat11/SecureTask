namespace SecureTaskApi.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }

}
