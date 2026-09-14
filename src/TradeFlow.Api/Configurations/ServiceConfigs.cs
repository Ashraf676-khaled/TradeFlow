using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Api.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services)
  {
    services.AddEndpointsApiExplorer();
    services.AddHttpContextAccessor();

    return services;
  }
}
