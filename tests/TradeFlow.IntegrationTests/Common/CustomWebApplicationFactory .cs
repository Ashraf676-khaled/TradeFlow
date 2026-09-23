namespace TradeFlow.Api.IntegrationTests.Common;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradeFlow.Infrastructure.Data;
using TradeFlow.Infrastructure.Data.Interceptors;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
  private readonly SqliteConnection _connection = new("DataSource=:memory:");

  protected override IHost CreateHost(IHostBuilder builder)
  {
    // نحقن الإعدادات هنا، في أبكر مرحلة ممكنة قبل ما Program.cs يبدأ ينفذ حتى
    builder.ConfigureAppConfiguration((context, config) =>
    {
      config.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Jwt:Secret"] = "TestSecretKeyForIntegrationTestsOnly_MustBeLongEnough123!",
        ["Jwt:Issuer"] = "TradeFlow.Tests",
        ["Jwt:Audience"] = "TradeFlow.Tests",
        ["Jwt:ExpiryInMinutes"] = "15",
        ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:"
      });
    });

    return base.CreateHost(builder);
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      var descriptorsToRemove = services
          .Where(d =>
              d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
              d.ServiceType == typeof(DbContextOptions) ||
              d.ServiceType == typeof(AppDbContext) ||
              (d.ServiceType.FullName?.Contains("EntityFrameworkCore.SqlServer") ?? false) ||
              (d.ServiceType.FullName?.Contains("EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration") ?? false))
          .ToList();

      foreach (var d in descriptorsToRemove)
        services.Remove(d);

      // Integration tests use SQLite; Hangfire's SQL Server worker must not start against that connection.
      var hangfireHostedServices = services
          .Where(d => d.ServiceType == typeof(IHostedService) &&
              (d.ImplementationType?.Namespace?.Contains("Hangfire", StringComparison.OrdinalIgnoreCase) == true ||
               d.ImplementationFactory?.Method.DeclaringType?.Namespace?.Contains("Hangfire", StringComparison.OrdinalIgnoreCase) == true))
          .ToList();
      foreach (var descriptor in hangfireHostedServices)
        services.Remove(descriptor);

      // تسجيل الـ Interceptor صراحة في الـ Test DI Container
      services.AddScoped<DispatchDomainEventsInterceptor>();

      _connection.Open();

      // إضافة الـ Interceptor للـ DbContext Options مع الـ SQLite
      services.AddDbContext<AppDbContext>((sp, options) =>
      {
        var interceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();
        options.UseSqlite(_connection)
               .AddInterceptors(interceptor);
      });

      using var scope = services.BuildServiceProvider().CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      context.Database.EnsureCreated();
    });
  }

  protected override void Dispose(bool disposing)
  {
    _connection.Dispose();
    base.Dispose(disposing);
  }
}
