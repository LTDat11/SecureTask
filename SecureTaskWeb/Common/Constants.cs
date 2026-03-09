namespace SecureTaskWeb.Common;

/// <summary>
/// Constants for API endpoints
/// </summary>
public static class ApiEndpoints
{
    public const string LoginEndpoint = "/api/auth/login";
    public const string RegisterEndpoint = "/api/auth/register";
    public const string TasksEndpoint = "/api/tasks";
    public const string AdminEndpoint = "/api/admin";
}

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    public const string JwtCookieName = "jwt";
    public const int JwtCookieDays = 7;
    public const string DefaultRole = "User";
    public const string AdminRole = "Admin";
}
