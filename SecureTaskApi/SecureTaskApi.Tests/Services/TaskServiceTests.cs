using Moq;
using Xunit;
using SecureTaskApi.DTOs;
using SecureTaskApi.Entities;
using SecureTaskApi.Exceptions;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Implementations;

namespace SecureTaskApi.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly TaskService _taskService;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _taskService = new TaskService(_taskRepoMock.Object);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsTaskResponse()
    {
        _taskRepoMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);
        _taskRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var request = new CreateTaskRequest
        {
            Title = "Write unit tests",
            Description = "Cover service layer",
            Deadline = DateTime.UtcNow.AddDays(3)
        };

        var result = await _taskService.CreateAsync(request, _userId);

        Assert.Equal("Write unit tests", result.Title);
        Assert.Equal("Cover service layer", result.Description);
        Assert.Equal("Todo", result.Status);
        _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullDeadline_ReturnsTaskWithNullDeadline()
    {
        _taskRepoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        _taskRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new CreateTaskRequest { Title = "No deadline task" };

        var result = await _taskService.CreateAsync(request, _userId);

        Assert.Null(result.Deadline);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        _taskRepoMock
            .Setup(r => r.GetByIdAndUserAsync(taskId, _userId))
            .ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Deadline = DateTime.UtcNow.AddDays(2),
            Status = Entities.TaskStatus.Doing
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _taskService.UpdateAsync(taskId, request, _userId));
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesTaskFields()
    {
        var taskId = Guid.NewGuid();
        var existing = new TaskItem
        {
            Id = taskId,
            Title = "Old title",
            Status = Entities.TaskStatus.Todo,
            UserId = _userId
        };

        _taskRepoMock
            .Setup(r => r.GetByIdAndUserAsync(taskId, _userId))
            .ReturnsAsync(existing);
        _taskRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var newDeadline = DateTime.UtcNow.AddDays(5);
        var request = new UpdateTaskRequest
        {
            Title = "New title",
            Description = "New description",
            Deadline = newDeadline,
            Status = Entities.TaskStatus.Done
        };

        await _taskService.UpdateAsync(taskId, request, _userId);

        Assert.Equal("New title", existing.Title);
        Assert.Equal("New description", existing.Description);
        Assert.Equal(Entities.TaskStatus.Done, existing.Status);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_TaskNotFound_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        _taskRepoMock
            .Setup(r => r.GetByIdAndUserAsync(taskId, _userId))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _taskService.DeleteAsync(taskId, _userId));
    }

    [Fact]
    public async Task DeleteAsync_ValidTask_CallsDeleteAndSave()
    {
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "To delete", UserId = _userId };

        _taskRepoMock
            .Setup(r => r.GetByIdAndUserAsync(taskId, _userId))
            .ReturnsAsync(task);
        _taskRepoMock
            .Setup(r => r.DeleteAsync(task))
            .Returns(Task.CompletedTask);
        _taskRepoMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        await _taskService.DeleteAsync(taskId, _userId);

        _taskRepoMock.Verify(r => r.DeleteAsync(task), Times.Once);
        _taskRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
