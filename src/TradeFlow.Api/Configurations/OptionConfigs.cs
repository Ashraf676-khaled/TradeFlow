using TradeFlow.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Api.Configurations;

public static class OptionConfigs
{
  public static IServiceCollection AddOptionConfigs(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.Configure<Jwt>(configuration.GetSection("Jwt"));

    return services;
  }
}
