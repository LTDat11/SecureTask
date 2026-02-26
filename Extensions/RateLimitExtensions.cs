using Microsoft.AspNetCore.RateLimiting;
namespace SecureTaskApi.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddCustomRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("LoginPolicy", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5;
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    status = 429,
                    message = "Too many login attempts."
                }, token);
            };
        });

        return services;
    }
}