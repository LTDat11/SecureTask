using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
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

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tasks = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.ToString(),
                Description = t.Description,
                Deadline = t.Deadline
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // GET: api/tasks/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var task = await _context.TaskItems
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(task => new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status.ToString(),
                Description = task.Description,
                Deadline = task.Deadline
            })
            .FirstOrDefaultAsync();

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    // GET: api/tasks/status/{status}
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetTasksByStatus(Entities.TaskStatus status)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tasks = await _context.TaskItems
            .Where(t => t.UserId == userId && t.Status == status)
            .Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.ToString(),
                Description = t.Description,
                Deadline = t.Deadline
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // GET: api/tasks/sort?title=abc&status=Todo&deadline=2024-06-30&sortBy=Deadline&sortOrder=desc&page=1&pageSize=10
    [HttpGet("sort")]
    public async Task<IActionResult> GetTasks([FromQuery] TaskQuery query)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tasksQuery = _context.TaskItems
            .AsNoTracking()
            .Where(t => t.UserId == userId);

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

        tasksQuery = tasksQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var tasks = await tasksQuery
            .Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.ToString(),
                Description = t.Description,
                Deadline = t.Deadline
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest taskrequest)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        var task = new TaskItem
        {
            Title = taskrequest.Title,
            Description = taskrequest.Description,
            Deadline = taskrequest.Deadline?.ToUniversalTime(),
            UserId = user.Id
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
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var task = await _context.TaskItems
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return NotFound();

        task.Title = taskRequest.Title;
        task.Description = taskRequest.Description;
        task.Deadline = taskRequest.Deadline.ToUniversalTime();
        task.Status = (Entities.TaskStatus)taskRequest.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var task = await _context.TaskItems
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
            return NotFound();

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
