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
        var username = User.FindFirstValue(ClaimTypes.Name);

        var tasks = await _context.TaskItems
            .Where(t => t.User.UserName == username)
            .Select(t => new TaskResponseDto
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
    public async Task<IActionResult> CreateTask(CreateTaskDto dto)
    {
        var username = User.FindFirstValue(ClaimTypes.Name);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
            return NotFound();

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Deadline = dto.Deadline,
            UserId = user.Id
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyTasks), new { id = task.Id }, new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status.ToString(),
            Description = task.Description,
            Deadline = task.Deadline
        });
    }
}
