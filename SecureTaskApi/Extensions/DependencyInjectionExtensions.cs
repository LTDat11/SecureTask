using SecureTaskApi.Repositories.Implementations;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Services.Implementations;
using SecureTaskApi.Services.Interfaces;

namespace SecureTaskApi.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}