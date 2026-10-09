using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using TradeFlow.Api;
using TradeFlow.Api.Configurations;
using TradeFlow.Api.Extensions;
using TradeFlow.Api.Health;
using TradeFlow.Api.Middlewares;
using TradeFlow.Application;
using TradeFlow.Infrastructure;
using TradeFlow.Infrastructure.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Serilog لازم يتسجل الأول قبل أي حاجة، عشان يلقط أي Exception حتى وقت الـ Startup
builder.AddLoggerConfigs();
builder.Services.AddOpenApi();
Log.Information("Starting up TradeFlow.Api");

// Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services
  .AddHealthChecks()
  .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
  .AddCheck<HangfireStorageHealthCheck>("background-jobs", tags: ["ready"])
  .AddTradeFlowFeatureHealthChecks();

// Api layer (DependencyInjection.cs)
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
  {
    policy.WithOrigins("https://trade-flow-dashboard-one.vercel.app")
          .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
          .WithHeaders("Authorization", "Content-Type");
  });
});

var app = builder.Build();

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
  context.Response.ContentType = "application/json";
  return context.Response.WriteAsync(JsonSerializer.Serialize(new
  {
    status = report.Status.ToString(),
    checkedAtUtc = DateTimeOffset.UtcNow,
    totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
    summary = new
    {
      total = report.Entries.Count,
      healthy = report.Entries.Count(entry => entry.Value.Status == HealthStatus.Healthy),
      degraded = report.Entries.Count(entry => entry.Value.Status == HealthStatus.Degraded),
      unhealthy = report.Entries.Count(entry => entry.Value.Status == HealthStatus.Unhealthy)
    },
    checks = report.Entries.OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry => new
    {
      name = entry.Key,
      status = entry.Value.Status.ToString(),
      description = entry.Value.Description,
      durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
      data = entry.Value.Data
    })
  }));
}

// 0. CORS لازم تكون في البداية خالص قبل أي Middleware تاني عشان الـ Preflight Requests (OPTIONS) تعدي
app.UseCors("AllowFrontend");

// 1. Exception Handling & Logging
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseRequestLogContext();

// 2. Development Tools (Scalar & OpenAPI) & Hangfire Dashboard
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference(options =>
  {
    options.Title = "TradeFlow API Documentation";
    options.Theme = ScalarTheme.Purple;
  });

  // لو حابب تخلي لوحة تحكم هانجفاير متاحة في الـ Development فقط (أو تشيل الشرط لو عايزها في كل البيئات)
  app.UseHangfireDashboard("/hangfire");
}

// تسجيل وإ جدولة الـ Recurring Jobs (يتم تخطيها في بيئة الـ Testing لعدم التأثير على الاختبارات)
if (!app.Environment.IsEnvironment("Testing"))
{
  app.UseHttpsRedirection();

  // تسجيل وإ جدولة الـ Recurring Jobs عند بدء التطبيق في الإنتاج/التطوير الفعلي
  app.RegisterRecurringJobs();
}

// 3. Security & Routing (لازم بعد الـ CORS وقبل الـ Endpoints)
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
  Predicate = _ => false,
  ResponseWriter = WriteHealthResponse
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready"),
  ResponseWriter = WriteHealthResponse
}).AllowAnonymous();
app.MapHealthChecks("/health", new HealthCheckOptions
{
  ResponseWriter = WriteHealthResponse
}).AllowAnonymous();

// 4. Endpoints Mapping (دي لازم تكون آخر حاجة قبل الـ Run)
app.MapEndpoints();

try
{
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "TradeFlow.Api terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}

public partial class Program { }
