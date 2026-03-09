namespace SecureTaskWeb.Models;

/// <summary>
/// User session data stored in session
/// </summary>
public class UserSession
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// Check if token is still valid
    /// </summary>
    public bool IsTokenValid => DateTime.UtcNow < TokenExpiry;

    /// <summary>
    /// Get time remaining in seconds before token expires
    /// </summary>
    public int SecondsUntilExpiry => (int)(TokenExpiry - DateTime.UtcNow).TotalSeconds;
}
