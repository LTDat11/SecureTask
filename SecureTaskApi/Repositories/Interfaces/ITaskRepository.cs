using SecureTaskApi.Entities;

namespace SecureTaskApi.Repositories.Interfaces;

public interface ITaskRepository
{
    IQueryable<TaskItem> Query();
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<TaskItem?> GetByIdAndUserAsync(Guid id, Guid userId);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
    Task SaveChangesAsync();
}