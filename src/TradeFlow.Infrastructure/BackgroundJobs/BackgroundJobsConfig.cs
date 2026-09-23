namespace TradeFlow.Infrastructure.BackgroundJobs;

using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeFlow.Application.Common.Interfaces;

public static class BackgroundJobsConfig
{
  public static IServiceCollection AddBackgroundJobs(
      this IServiceCollection services, IConfiguration configuration)
  {
    services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
              QueuePollInterval = TimeSpan.Zero,
              UseRecommendedIsolationLevel = true,
              DisableGlobalLocks = true
            }));

    // Worker واحد كافي للمرحلة دي؛ زوّد WorkerCount لو الأحمال زادت مستقبلًا
    services.AddHangfireServer(options =>
    {
      options.WorkerCount = 1;
      options.Queues = ["default"];
    });

    services.AddScoped<IHangfireJobService, HangfireJobService>();

    return services;
  }

  /// <summary>
  /// يسجل الـRecurring Jobs عند بدء تشغيل التطبيق. تُستدعى مرة واحدة من Program.cs بعد app.Build().
  /// </summary>
  public static void RegisterRecurringJobs(this IApplicationBuilder app)
  {
    RecurringJob.AddOrUpdate<IHangfireJobService>(
        "cleanup-expired-refresh-tokens",
        job => job.CleanupExpiredRefreshTokensAsync(CancellationToken.None),
        Cron.Daily(3)); // كل يوم الساعة 3 صباحًا

    RecurringJob.AddOrUpdate<IHangfireJobService>(
        "check-low-stock-levels",
        job => job.CheckLowStockLevelsAsync(CancellationToken.None),
        Cron.Hourly());

    RecurringJob.AddOrUpdate<IHangfireJobService>(
        "check-overdue-invoices",
        job => job.CheckOverdueInvoicesAsync(CancellationToken.None),
        Cron.Daily(2)); // كل يوم الساعة 2 صباحًا
  }
}
