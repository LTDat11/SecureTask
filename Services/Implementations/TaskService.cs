
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Services.Implementations;

public class TaskService : ITaskService
{
    // injecting the task repository to handle data access
    private readonly ITaskRepository _taskRepository;

    // constructor to initialize the task repository
    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    // Mapping function to convert TaskItem to TaskResponse 
    private static Expression<Func<TaskItem, TaskResponse>> MapToResponse()
    {
        return t => new TaskResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status.ToString(),
            Deadline = t.Deadline
        };
    }

    // function to create a new task for a user
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid userId)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline?.ToUniversalTime(),
            UserId = userId
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Deadline = task.Deadline
        };
    }

    // function to update an existing task for a user
    public async Task UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetByIdAndUserAsync(id, userId);

        if (task == null)
            throw new NotFoundException("Task not found");

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.Deadline = request.Deadline.ToUniversalTime();

        await _taskRepository.SaveChangesAsync();
    }

    // function to get a specific task by id for a user
    public async Task<TaskResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.Query()
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(MapToResponse())
            .FirstOrDefaultAsync();

        if (task == null)
            throw new NotFoundException("Task not found");

        return task;
    }

    // function to get all tasks for a user
    public async Task<List<TaskResponse>> GetMyTasksAsync(Guid userId)
    {
        return await _taskRepository.Query()
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(MapToResponse())
            .ToListAsync();
    }

    // function to get paginated, filtered, and sorted list of tasks for a user
    public async Task<PagedResponse<TaskResponse>> GetTasksAsync(TaskQuery query, Guid userId)
    {
        if (query.Page <= 0) query.Page = 1;
        if (query.PageSize <= 0 || query.PageSize > 100) query.PageSize = 10;

        var tasksQuery = _taskRepository.Query()
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        // FILTER
        if (!string.IsNullOrWhiteSpace(query.Title))
            tasksQuery = tasksQuery.Where(t =>
                EF.Functions.ILike(t.Title, $"%{query.Title}%"));

        if (query.Status.HasValue)
            tasksQuery = tasksQuery.Where(t =>
                t.Status == query.Status.Value);

        // SORT
        var isDesc = query.SortOrder?.ToLower() == "desc";

        tasksQuery = query.SortBy switch
        {
            TaskSortBy.Deadline =>
                isDesc ? tasksQuery.OrderByDescending(t => t.Deadline)
                       : tasksQuery.OrderBy(t => t.Deadline),

            _ =>
                isDesc ? tasksQuery.OrderByDescending(t => t.Title)
                       : tasksQuery.OrderBy(t => t.Title)
        };

        var totalItems = await tasksQuery.CountAsync();

        var items = await tasksQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToResponse())
            .ToListAsync();

        return new PagedResponse<TaskResponse>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize),
            PageNumber = query.Page,
            PageSize = query.PageSize
        };
    }

    // function to delete a task by id for a user
    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetByIdAndUserAsync(id, userId);

        if (task == null)
            throw new NotFoundException("Task not found");

        await _taskRepository.DeleteAsync(task);
        await _taskRepository.SaveChangesAsync();
    }
}