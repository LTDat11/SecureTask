using SecureTaskApi.DTOs;

namespace SecureTaskApi.Services.Interfaces;

public interface ITaskService
{
    Task<List<TaskResponse>> GetMyTasksAsync(Guid userId);
    Task<TaskResponse> GetByIdAsync(Guid id, Guid userId);
    Task<PagedResponse<TaskResponse>> GetTasksAsync(TaskQuery query, Guid userId);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId);
    Task UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}