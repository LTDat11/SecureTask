using System.ComponentModel.DataAnnotations;

namespace SecureTaskApi.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string UserName { get; set; } = default!;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

}