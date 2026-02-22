using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // function to get user id from claims
    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // function to map TaskItem to TaskResponse
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

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = GetUserId();

        var tasks = await _context.TaskItems
            .AsNoTracking() // to improve performance since we are only reading data
            .Where(t => t.UserId == userId)
            .Select(MapToResponse())
            .ToListAsync();

        return Ok(tasks);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = GetUserId();

        var task = await _context.TaskItems
            .AsNoTracking() // to improve performance since we are only reading data
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(MapToResponse())
            .FirstOrDefaultAsync();

        if (task == null)
            throw new NotFoundException("Task not found");

        return Ok(task);
    }

    // GET: api/tasks/status/{status}
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetTasksByStatus(Entities.TaskStatus status)
    {
        var userId = GetUserId();

        var tasks = await _context.TaskItems
            .AsNoTracking() // to improve performance since we are only reading data
            .Where(t => t.UserId == userId && t.Status == status)
            .Select(MapToResponse())
            .ToListAsync();

        var response = new PagedResponse<TaskResponse>
        {
            Items = tasks,
            TotalItems = tasks.Count,
            PageNumber = 1,
            PageSize = tasks.Count
        };

        return Ok(response);
    }

    // GET: api/tasks/sort?title=abc&status=Todo&deadline=2024-06-30&sortBy=Deadline&sortOrder=desc&page=1&pageSize=10
    [HttpGet("sort")]
    public async Task<IActionResult> GetTasks([FromQuery] TaskQuery query)
    {
        var userId = GetUserId();

        // Validate pagination input
        if (query.Page <= 0) query.Page = 1;
        if (query.PageSize <= 0 || query.PageSize > 100) query.PageSize = 10;

        var tasksQuery = _context.TaskItems
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        // Filtering
        if (query.Deadline.HasValue)
        {
            var date = DateTime.SpecifyKind(query.Deadline.Value.Date, DateTimeKind.Utc);
            var nextDate = date.AddDays(1);

            tasksQuery = tasksQuery.Where(t =>
                t.Deadline.HasValue &&
                t.Deadline >= date &&
                t.Deadline < nextDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Title))
            tasksQuery = tasksQuery
                .Where(t => EF.Functions.ILike(t.Title, $"%{query.Title}%"));

        if (!string.IsNullOrWhiteSpace(query.Description))
            tasksQuery = tasksQuery
                .Where(t => EF.Functions.ILike(t.Description!, $"%{query.Description}%"));

        if (query.Status.HasValue)
            tasksQuery = tasksQuery
                .Where(t => t.Status == query.Status.Value);

        // Sorting
        var isDesc = query.SortOrder?.ToLower() == "desc";

        tasksQuery = query.SortBy switch
        {
            TaskSortBy.Deadline =>
                isDesc ? tasksQuery.OrderByDescending(t => t.Deadline)
                       : tasksQuery.OrderBy(t => t.Deadline),

            TaskSortBy.Status =>
                isDesc ? tasksQuery.OrderByDescending(t => t.Status)
                       : tasksQuery.OrderBy(t => t.Status),

            _ =>
                isDesc ? tasksQuery.OrderByDescending(t => t.Title)
                       : tasksQuery.OrderBy(t => t.Title)
        };

        // Count BEFORE pagination
        var totalItems = await tasksQuery.CountAsync();

        // Apply pagination
        var pagedQuery = tasksQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var items = await pagedQuery
            .Select(MapToResponse())
            .ToListAsync();

        var response = new PagedResponse<TaskResponse>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize),
            PageNumber = query.Page,
            PageSize = query.PageSize
        };

        return Ok(response);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest taskrequest)
    {
        var userId = GetUserId();

        var task = new TaskItem
        {
            Title = taskrequest.Title,
            Description = taskrequest.Description,
            Deadline = taskrequest.Deadline?.ToUniversalTime(),
            UserId = userId
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyTasks), new { id = task.Id }, new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status.ToString(),
            Description = task.Description,
            Deadline = task.Deadline
        });
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid id, UpdateTaskRequest taskRequest)
    {
        var userId = GetUserId();

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            throw new NotFoundException("Task not found");

        task.Title = taskRequest.Title;
        task.Description = taskRequest.Description;
        task.Deadline = taskRequest.Deadline.ToUniversalTime();
        task.Status = taskRequest.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetUserId();

        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            throw new NotFoundException("Task not found");

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
