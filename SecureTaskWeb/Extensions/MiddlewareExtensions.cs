using SecureTaskWeb.Middlewares;

namespace SecureTaskWeb.Extensions;

/// <summary>
/// Extension methods for configuring middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds custom error handling middleware
    /// </summary>
    public static WebApplication UseApplicationErrorHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Add global exception middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();

        return app;
    }

    /// <summary>
    /// Configures the standard request/response pipeline
    /// </summary>
    public static WebApplication UseApplicationDefaults(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseSession();

        // Add token expiry check middleware
        app.UseMiddleware<TokenExpiryMiddleware>();

        return app;
    }
}
