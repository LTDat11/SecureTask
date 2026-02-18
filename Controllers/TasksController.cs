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
    public async Task<IActionResult> GetTasksByStatus(string status)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!Enum.TryParse<Entities.TaskStatus>(status, true, out var taskStatus))
            return BadRequest("Invalid status");

        var tasks = await _context.TaskItems
            .Where(t => t.UserId == userId && t.Status == taskStatus)
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

    // GET: api/tasks?dateline=2026-01-01&sortBy=Date&sortOrder=asc
    [HttpGet("sort")]
    public async Task<IActionResult> GetSortedTasks(DateTime? deadline, string? sortBy, string? sortOrder = "asc")
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tasksQuery = _context.TaskItems
            .Where(t => t.UserId == userId);

        // Filter by deadline (date without time component)
        if (deadline.HasValue)
        {
            var date = DateTime.SpecifyKind(deadline.Value.Date, DateTimeKind.Utc);
            var nextDate = date.AddDays(1);

            tasksQuery = tasksQuery.Where(t =>
                t.Deadline.HasValue &&
                t.Deadline.Value >= date &&
                t.Deadline.Value < nextDate);
        }

        // sanitze input
        sortBy = sortBy?.ToLower();
        sortOrder = sortOrder?.ToLower() ?? "asc";

        // Sorting
        tasksQuery = sortBy switch
        {
            "date" => sortOrder == "desc"
            ? tasksQuery.OrderByDescending(t => t.Deadline)
            : tasksQuery.OrderBy(t => t.Deadline),

            "status" => sortOrder == "desc"
                ? tasksQuery.OrderByDescending(t => t.Status)
                : tasksQuery.OrderBy(t => t.Status),

            "title" => sortOrder == "desc"
                ? tasksQuery.OrderByDescending(t => t.Title)
                : tasksQuery.OrderBy(t => t.Title),

            _ => tasksQuery.OrderBy(t => t.Title)
        };

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
    public async Task<IActionResult> CreateTask(CreateTaskRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Deadline = dto.Deadline?.ToUniversalTime(),
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
}
