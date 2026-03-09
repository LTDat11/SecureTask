using SecureTaskWeb.Helpers;
using SecureTaskWeb.Models;

namespace SecureTaskWeb.Middlewares;

/// <summary>
/// Middleware to check if user's JWT token has expired
/// If expired, removes session and redirects to login
/// </summary>
public class TokenExpiryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenExpiryMiddleware> _logger;

    public TokenExpiryMiddleware(RequestDelegate next, ILogger<TokenExpiryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip check for Auth controllers, Home, and static files
        if (!context.Request.Path.StartsWithSegments("/Auth") &&
            !context.Request.Path.StartsWithSegments("/Home") &&
            !context.Request.Path.StartsWithSegments("/api") &&
            !context.Request.Path.StartsWithSegments("/lib") &&
            !context.Request.Path.StartsWithSegments("/css") &&
            !context.Request.Path.StartsWithSegments("/js"))
        {
            // Check if user has session
            var userSession = context.Session.Get<UserSession>("UserSession");

            if (userSession != null)
            {
                // Check if token has expired
                if (JwtHelper.IsTokenExpired(userSession.Token))
                {
                    _logger.LogInformation("Token expired for user {Username}", userSession.Username);

                    // Clear session
                    context.Session.Remove("UserSession");
                    context.Session.Clear();

                    // Redirect to login
                    context.Response.Redirect("/Auth/Login?expired=true");
                    return;
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for session operations
/// </summary>
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value) where T : class
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key) where T : class
    {
        var value = session.GetString(key);
        return value == null ? null : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
}
