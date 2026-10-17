using Microsoft.Extensions.Diagnostics.HealthChecks;
using PasswordManager.Infrastructure.Persistence;

namespace PasswordManager.Api.Health;

/// <summary>
/// Health check that verifies real-time connectivity to the database.
/// Compatible with both PostgreSQL production instances and InMemory test instances.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy.")
                : HealthCheckResult.Unhealthy("Cannot connect to database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check encountered an error.", ex);
        }
    }
}
