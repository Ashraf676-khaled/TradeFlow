using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradeFlow.Api.Health;

public sealed class HangfireStorageHealthCheck(
    JobStorage jobStorage,
    IHostEnvironment environment,
    ILogger<HangfireStorageHealthCheck> logger) : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var monitoringApi = jobStorage.GetMonitoringApi();
      monitoringApi.GetStatistics();
      if (environment.IsProduction() && monitoringApi.Servers().Count == 0)
      {
        return Task.FromResult(HealthCheckResult.Degraded(
            "No Hangfire server is registered; background jobs cannot run.",
            data: new Dictionary<string, object>
            {
              ["component"] = "Hangfire",
              ["impact"] = "Queued and scheduled jobs are not being processed."
            }));
      }

      return Task.FromResult(HealthCheckResult.Healthy(
          "Background-job storage is reachable.",
          data: new Dictionary<string, object>
          {
            ["component"] = "Hangfire",
            ["impact"] = "Scheduled jobs can run."
          }));
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Hangfire storage health check failed.");
      return Task.FromResult(HealthCheckResult.Degraded(
          "Background-job storage is unavailable; scheduled jobs may be delayed.",
          data: new Dictionary<string, object>
          {
            ["component"] = "Hangfire",
            ["impact"] = "Scheduled jobs are unavailable."
          }));
    }
  }
}