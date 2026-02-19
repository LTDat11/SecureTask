using SecureTaskApi.Entities;
namespace SecureTaskApi.DTOs;

public class TaskQuery
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public Entities.TaskStatus? Status { get; set; }

    public DateTime? Deadline { get; set; }

    public TaskSortBy? SortBy { get; set; }

    public string SortOrder { get; set; } = "asc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}