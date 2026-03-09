namespace SecureTaskWeb.Extensions;

/// <summary>
/// Extension methods for configuring logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds console logging with structured output
    /// </summary>
    public static WebApplicationBuilder AddApplicationLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
        }
        else
        {
            builder.Logging.SetMinimumLevel(LogLevel.Information);
        }

        return builder;
    }
}
