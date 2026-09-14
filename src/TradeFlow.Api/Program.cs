using TradeFlow.Api;
using TradeFlow.Api.Configurations;
using TradeFlow.Api.Extensions;
using TradeFlow.Api.Middlewares;
using TradeFlow.Application;
using TradeFlow.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog لازم يتسجل الأول قبل أي حاجة، عشان يلقط أي Exception حتى وقت الـ Startup
builder.AddLoggerConfigs();
builder.Services.AddOpenApi();
try
{
  Log.Information("Starting up TradeFlow.Api");

  // Clean Architecture layers
  builder.Services.AddApplication();
  builder.Services.AddInfrastructure(builder.Configuration);

  // Api layer (كل حاجة مجمعة دلوقتي في DependencyInjection.cs الجديد)
  builder.Services.AddApiServices(builder.Configuration);

  var app = builder.Build();


  app.UseExceptionHandler();
  app.MapOpenApi();
  app.MapScalarApiReference();
  app.UseSerilogRequestLogging();
  app.UseRequestLogContext();


  app.UseHttpsRedirection();
  app.UseAuthentication();
  app.UseAuthorization();

  app.MapEndpoints();

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
