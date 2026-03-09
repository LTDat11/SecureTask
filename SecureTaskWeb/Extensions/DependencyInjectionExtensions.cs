using SecureTaskWeb.Services;
using SecureTaskWeb.Services.Interfaces;

namespace SecureTaskWeb.Extensions;

/// <summary>
/// Extension methods for registering application services in the dependency injection container
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register API client and services
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"];

        services.AddHttpClient<IApiClient, ApiClientBase>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl ?? throw new InvalidOperationException("ApiSettings:BaseUrl not configured"));
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IAuthService, AuthApiService>();
        services.AddScoped<ITaskService, TaskApiService>();

        return services;
    }

    /// <summary>
    /// Configures Mvc options for the application
    /// </summary>
    public static IServiceCollection AddApplicationMvc(this IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        return services;
    }
}
