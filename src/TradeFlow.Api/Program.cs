using Hangfire;
using Scalar.AspNetCore;
using Serilog;
using TradeFlow.Api;
using TradeFlow.Api.Configurations;
using TradeFlow.Api.Extensions;
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

// Api layer (DependencyInjection.cs)
builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowFrontend", policy =>
  {
    policy.WithOrigins("http://localhost:5001", "http://localhost:5174", "http://localhost:5175", "http://localhost:5176")
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});

var app = builder.Build();

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
