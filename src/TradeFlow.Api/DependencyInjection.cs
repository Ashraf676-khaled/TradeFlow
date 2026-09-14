using TradeFlow.Api.Configurations;
using TradeFlow.Api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Api;

public static class DependencyInjection
{
  public static IServiceCollection AddApiServices(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddOptionConfigs(configuration);
    services.AddServiceConfigs();
    services.AddAuthenticationConfig(configuration);
    services.AddMediatorConfig();

    services.AddExceptionHandler<GlobalExceptionHandler>();
    services.AddProblemDetails();

    return services;
  }
}
