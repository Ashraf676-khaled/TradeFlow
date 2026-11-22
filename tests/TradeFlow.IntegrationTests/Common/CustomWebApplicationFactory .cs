namespace TradeFlow.Api.IntegrationTests.Common;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TradeFlow.Infrastructure.Data;
using TradeFlow.Infrastructure.Data.Interceptors;
using TradeFlow.Infrastructure.Identity;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
  private const string TestJwtSecret = "TestSecretKeyForIntegrationTestsOnly_MustBeLongEnough123!";
  private const string TestJwtIssuer = "TradeFlow.Tests";
  private const string TestJwtAudience = "TradeFlow.Tests";
  private static readonly object EnvironmentLock = new();
  private readonly string _connectionString = new SqliteConnectionStringBuilder
  {
    DataSource = $"TradeFlowTests_{Guid.NewGuid():N}",
    Mode = SqliteOpenMode.Memory,
    Cache = SqliteCacheMode.Shared
  }.ToString();
  private readonly SqliteConnection _connection;
  private readonly int _authPermitLimit;

  public CustomWebApplicationFactory() : this(1000) { }

  protected CustomWebApplicationFactory(int authPermitLimit)
  {
    _authPermitLimit = authPermitLimit;
    _connection = new SqliteConnection(_connectionString);
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    lock (EnvironmentLock)
    {
      var originalSecret = Environment.GetEnvironmentVariable("Jwt__Secret");
      var originalIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
      var originalAudience = Environment.GetEnvironmentVariable("Jwt__Audience");
      var originalExpiry = Environment.GetEnvironmentVariable("Jwt__ExpiryInMinutes");
      var originalAuthPermitLimit = Environment.GetEnvironmentVariable("RateLimit__AuthPermitLimit");
      var originalPlatformAdminUserId = Environment.GetEnvironmentVariable("PlatformAdmin__UserId");
      try
      {
        Environment.SetEnvironmentVariable("Jwt__Secret", TestJwtSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", TestJwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", TestJwtAudience);
        Environment.SetEnvironmentVariable("Jwt__ExpiryInMinutes", "15");
        Environment.SetEnvironmentVariable("RateLimit__AuthPermitLimit", _authPermitLimit.ToString());
        Environment.SetEnvironmentVariable("PlatformAdmin__UserId", string.Empty);
        return base.CreateHost(builder);
      }
      finally
      {
        Environment.SetEnvironmentVariable("Jwt__Secret", originalSecret);
        Environment.SetEnvironmentVariable("Jwt__Issuer", originalIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", originalAudience);
        Environment.SetEnvironmentVariable("Jwt__ExpiryInMinutes", originalExpiry);
        Environment.SetEnvironmentVariable("RateLimit__AuthPermitLimit", originalAuthPermitLimit);
        Environment.SetEnvironmentVariable("PlatformAdmin__UserId", originalPlatformAdminUserId);
      }
    }
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");
    builder.ConfigureAppConfiguration((_, configuration) =>
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["PlatformAdmin:Email"] = "platform-admin@test.com",
          ["PlatformAdmin:UserId"] = string.Empty,
          ["RateLimit:AuthPermitLimit"] = _authPermitLimit.ToString()
        }));
    builder.ConfigureServices(services =>
    {
      services.PostConfigure<Jwt>(options =>
      {
        options.Secret = TestJwtSecret;
        options.Issuer = TestJwtIssuer;
        options.Audience = TestJwtAudience;
        options.ExpiryInMinutes = 15;
      });
      services.PostConfigure<PlatformAdminOptions>(options =>
      {
        options.UserId = null;
        options.Email = "platform-admin@test.com";
      });
      services.PostConfigure<JwtBearerOptions>(options =>
      {
        options.TokenValidationParameters.ValidIssuer = TestJwtIssuer;
        options.TokenValidationParameters.ValidAudience = TestJwtAudience;
        options.TokenValidationParameters.IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
      });

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
        options.UseSqlite(_connectionString)
               .AddInterceptors(interceptor);
      });

      using var provider = services.BuildServiceProvider();
      using var scope = provider.CreateScope();
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
