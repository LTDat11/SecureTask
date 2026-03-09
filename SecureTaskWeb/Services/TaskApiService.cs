using SecureTaskWeb.Common;
using SecureTaskWeb.Services.Interfaces;

namespace SecureTaskWeb.Services;

/// <summary>
/// Service for handling task operations with the backend API
/// </summary>
public class TaskApiService : ITaskService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<TaskApiService> _logger;

    public TaskApiService(IApiClient apiClient, ILogger<TaskApiService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<ApiResult<IEnumerable<TaskDto>>> GetTasksAsync()
    {
        _logger.LogInformation("Fetching all tasks");
        return await _apiClient.GetAsync<IEnumerable<TaskDto>>(ApiEndpoints.TasksEndpoint);
    }

    public async Task<ApiResult<TaskDto>> GetTaskAsync(Guid id)
    {
        _logger.LogInformation("Fetching task with ID: {TaskId}", id);
        return await _apiClient.GetAsync<TaskDto>($"{ApiEndpoints.TasksEndpoint}/{id}");
    }

    public async Task<ApiResult<TaskDto>> CreateTaskAsync(CreateTaskRequest request)
    {
        _logger.LogInformation("Creating new task: {Title}", request.Title);
        return await _apiClient.PostAsync<TaskDto>(ApiEndpoints.TasksEndpoint, request);
    }

    public async Task<ApiResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        _logger.LogInformation("Updating task with ID: {TaskId}", id);
        return await _apiClient.PutAsync<TaskDto>($"{ApiEndpoints.TasksEndpoint}/{id}", request);
    }

    public async Task<ApiResult<object>> DeleteTaskAsync(Guid id)
    {
        _logger.LogInformation("Deleting task with ID: {TaskId}", id);
        return await _apiClient.DeleteAsync<object>($"{ApiEndpoints.TasksEndpoint}/{id}");
    }
}
