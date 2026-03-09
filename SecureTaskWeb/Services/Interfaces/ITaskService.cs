namespace SecureTaskWeb.Services.Interfaces;

/// <summary>
/// Service for handling task-related operations
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Gets all tasks for the current user
    /// </summary>
    Task<ApiResult<IEnumerable<TaskDto>>> GetTasksAsync();

    /// <summary>
    /// Gets a specific task by ID
    /// </summary>
    Task<ApiResult<TaskDto>> GetTaskAsync(Guid id);

    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskRequest request);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<ApiResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);

    /// <summary>
    /// Deletes a task
    /// </summary>
    Task<ApiResult<object>> DeleteTaskAsync(Guid id);
}

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}
