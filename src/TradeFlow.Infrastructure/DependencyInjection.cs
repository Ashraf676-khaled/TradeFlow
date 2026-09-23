using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Infrastructure.BackgroundJobs;
using TradeFlow.Infrastructure.Data;
using TradeFlow.Infrastructure.Data.Interceptors;
using TradeFlow.Infrastructure.Identity;

namespace TradeFlow.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddHttpContextAccessor();

    services.Configure<Jwt>(configuration.GetSection("Jwt"));

    services.AddScoped<ICurrentUserService, CurrentUserService>();
    services.AddScoped<ITokenProvider, TokenProvider>();
    services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IIdentityService, IdentityService>();

    services.AddScoped<AuditableEntityInterceptor>();
    services.AddScoped<DispatchDomainEventsInterceptor>();

    services.AddDbContext<AppDbContext>((sp, options) =>
    {
      options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
      options.AddInterceptors(
          sp.GetRequiredService<AuditableEntityInterceptor>(),
          sp.GetRequiredService<DispatchDomainEventsInterceptor>());
    });
    services.AddBackgroundJobs(configuration);
    services.AddScoped<IApplicationDbContext>(sp =>
        sp.GetRequiredService<AppDbContext>());

    return services;
  }
}
