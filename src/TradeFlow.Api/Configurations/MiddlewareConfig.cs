using TradeFlow.Api.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Api.Configurations;

public static class MiddlewareConfig
{
  public static IServiceCollection AddMiddlewareConfig(this IServiceCollection services)
  {
    return services;
  }
}
