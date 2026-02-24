using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.Entities;
using SecureTaskApi.Repositories.Interfaces;

namespace SecureTaskApi.Repositories.Implementations;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TaskItem> Query()
    {
        return _context.TaskItems.AsQueryable();
    }

    public async Task<TaskItem?> GetByIdAndUserAsync(Guid id, Guid userId)
    {
        return await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task AddAsync(TaskItem task)
    {
        await _context.TaskItems.AddAsync(task);
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.TaskItems.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.TaskItems.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}