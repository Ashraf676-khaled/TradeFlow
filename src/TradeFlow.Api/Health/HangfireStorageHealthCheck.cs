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
      var statistics = monitoringApi.GetStatistics();
      var servers = monitoringApi.Servers();
      var data = new Dictionary<string, object>
      {
        ["component"] = "Hangfire",
        ["serverCount"] = servers.Count,
        ["workerCount"] = servers.Sum(server => server.WorkersCount),
        ["enqueuedJobs"] = statistics.Enqueued,
        ["failedJobs"] = statistics.Failed
      };

      if (environment.IsProduction() && servers.Count == 0)
      {
        return Task.FromResult(HealthCheckResult.Degraded(
            "No Hangfire server is registered; background jobs cannot run.",
            data: data));
      }

      if (statistics.Failed > 0)
      {
        logger.LogWarning("Hangfire has {FailedJobCount} failed jobs.", statistics.Failed);
        return Task.FromResult(HealthCheckResult.Degraded(
            "Hangfire has failed jobs; inspect the API logs and job storage.",
            data: data));
      }

      return Task.FromResult(HealthCheckResult.Healthy(
          "Background-job storage is reachable.",
          data));
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