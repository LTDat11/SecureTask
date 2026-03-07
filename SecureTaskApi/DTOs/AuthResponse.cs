namespace SecureTaskApi.DTOs;

public class AuthResponse
{
    public string UserName { get; set; } = default!;
    public string Token { get; set; } = default!;
}