using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradeFlow.Infrastructure.Data;

namespace TradeFlow.Api.Health;

public sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
  public async Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
      return canConnect
        ? HealthCheckResult.Healthy("Database is reachable.")
        : HealthCheckResult.Unhealthy("Database is unavailable.");
    }
    catch (Exception exception)
    {
      return HealthCheckResult.Unhealthy("Database is unavailable.", exception);
    }
  }
}
