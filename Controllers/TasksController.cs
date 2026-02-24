using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTaskApi.DTOs;
using SecureTaskApi.Services.Interfaces;
using System.Security.Claims;

namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    // function to get user id from claims
    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
        => Ok(await _service.GetMyTasksAsync(GetUserId()));

    // GET: api/tasks/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMyTask(Guid id)
        => Ok(await _service.GetByIdAsync(id, GetUserId()));

    // GET: api/tasks/sort?title=abc&status=Todo&deadline=2024-06-30&sortBy=Deadline&sortOrder=desc&page=1&pageSize=10
    [HttpGet("sort")]
    public async Task<IActionResult> GetTasks([FromQuery] TaskQuery query)
        => Ok(await _service.GetTasksAsync(query, GetUserId()));

    // POST: api/tasks
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        var result = await _service.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetMyTask), new { id = result.Id }, result);
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request)
    {
        await _service.UpdateAsync(id, request, GetUserId());
        return NoContent();
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, GetUserId());
        return NoContent();
    }
}
