namespace SecureTaskApi.DTOs;

public class MyTaskResponse
{
    public IEnumerable<TaskResponse> Items { get; set; } = new List<TaskResponse>();
    public int TotalItems { get; set; }
}