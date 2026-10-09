using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TradeFlow.Api.Health;

public sealed class HangfireStorageHealthCheck(JobStorage jobStorage) : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      jobStorage.GetMonitoringApi().GetStatistics();
      return Task.FromResult(HealthCheckResult.Healthy(
          "Background-job storage is reachable.",
          data: new Dictionary<string, object>
          {
            ["component"] = "Hangfire",
            ["impact"] = "Scheduled jobs can run."
          }));
    }
    catch (Exception)
    {
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