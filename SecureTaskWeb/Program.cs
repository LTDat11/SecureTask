using SecureTaskWeb.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.AddApplicationLogging();

// Add services to the container
builder.Services.AddApplicationMvc();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseApplicationErrorHandling();
app.UseApplicationDefaults();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();
