namespace TradeFlow.Infrastructure.BackgroundJobs;

using Hangfire;
using Hangfire.Common;
using Hangfire.MemoryStorage;
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
         .UseMemoryStorage()); // استخدام الذاكرة مباشرة من غير داتا بيس للـ Jobs

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
    var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
    var options = new RecurringJobOptions();

    recurringJobManager.AddOrUpdate(
        "cleanup-expired-refresh-tokens",
        Job.FromExpression<IHangfireJobService>(
            job => job.CleanupExpiredRefreshTokensAsync(CancellationToken.None)),
        Cron.Daily(3),
        options); // كل يوم الساعة 3 صباحًا

    recurringJobManager.AddOrUpdate(
        "check-low-stock-levels",
        Job.FromExpression<IHangfireJobService>(
            job => job.CheckLowStockLevelsAsync(CancellationToken.None)),
        Cron.Hourly(),
        options);

    recurringJobManager.AddOrUpdate(
        "check-overdue-invoices",
        Job.FromExpression<IHangfireJobService>(
            job => job.CheckOverdueInvoicesAsync(CancellationToken.None)),
        Cron.Daily(2),
        options); // كل يوم الساعة 2 صباحًا
  }
}
