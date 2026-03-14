using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Middlewares;
using Serilog;
using SecureTaskApi.Extensions;
using SecureTaskApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
builder.AddCustomLogging();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Use camelCase for JSON property names
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    // Serialize enums as strings in JSON responses
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer(); // Add API explorer for Swagger
builder.Services.AddSwaggerGen(); // Add Swagger for API documentation

// Add rate limiting
builder.Services.AddCustomRateLimit();

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
builder.Services.AddDatabase(builder.Configuration);

// JWT Authentication
builder.Services.AddJwtAuthentication();

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
builder.Services.AddApplicationServices();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Build after configuring services
var app = builder.Build();

app.UseSerilogRequestLogging();

// Enable CORS
app.UseCors("AllowAllOrigins");

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRateLimiter(); // Add rate limiting middleware

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health"); // Add health check endpoint

// Migrate database before running the application
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

