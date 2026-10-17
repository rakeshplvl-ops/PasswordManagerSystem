using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Api.Authentication;
using PasswordManager.Api.Middleware;
using PasswordManager.Application;
using PasswordManager.Infrastructure;
using PasswordManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Security: Disable Server header disclosure in Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Session Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = SessionAuthenticationOptions.SchemeName;
    options.DefaultChallengeScheme = SessionAuthenticationOptions.SchemeName;
})
.AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(
    SessionAuthenticationOptions.SchemeName, _ => { });

builder.Services.AddAuthorization();

// Configure CORS for React Vite Frontend & Docker Environments
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173",
                  "https://localhost:5173",
                  "http://localhost:3000",
                  "https://localhost:3000",
                  "http://localhost:8080",
                  "http://localhost",
                  "http://127.0.0.1:5173",
                  "http://127.0.0.1:3000",
                  "http://127.0.0.1")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Rate Limiting for Auth Endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthLimiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply database migrations on startup when running against real database (PostgreSQL)
if (!string.Equals(app.Configuration["DatabaseProvider"], "InMemory", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

// Export Program class for WebApplicationFactory integration tests
public partial class Program { }
