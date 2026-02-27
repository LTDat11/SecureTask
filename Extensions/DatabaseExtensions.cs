using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;

namespace SecureTaskApi.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? config.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}