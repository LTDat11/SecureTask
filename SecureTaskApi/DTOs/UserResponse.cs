namespace SecureTaskApi.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
