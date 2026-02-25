using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SecureTaskApi.Middlewares;
using SecureTaskApi.Services.Interfaces;
using SecureTaskApi.Services.Implementations;
using SecureTaskApi.Repositories.Interfaces;
using SecureTaskApi.Repositories.Implementations;
using Serilog;

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Serialize enums as strings in JSON responses
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer(); // Add API explorer for Swagger
builder.Services.AddSwaggerGen(); // Add Swagger for API documentation
builder.Host.UseSerilog(); // Use Serilog for logging

// Load ENV
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new Exception("JWT_KEY not set");

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? "SecureTaskApi";

var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? "SecureTaskApiUser";

// Use DATABASE_URL from environment variable if available, otherwise fall back to appsettings.json
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default");

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT
var key = Encoding.UTF8.GetBytes(jwtKey);

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Optional: Add events for responding to authentication failures, token validation, etc.
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Skip the default logic.
            context.HandleResponse();

            // Return a custom response for unauthorized requests.
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { error = "unauthorized", message = "Access token is missing or invalid" });
            return context.Response.WriteAsync(result);
        }
    };
});

// Swagger JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();


// Build after configuring services
var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())

    app.UseSerilogRequestLogging(); // Add Serilog request logging middleware

{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

