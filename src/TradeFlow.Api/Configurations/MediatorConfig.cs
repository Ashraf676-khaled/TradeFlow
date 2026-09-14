using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace TradeFlow.Api.Configurations;

public static class MediatorConfig
{
  public static IServiceCollection AddMediatorConfig(this IServiceCollection services)
  {
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(
            typeof(Application.IAssemblyMarker).Assembly));

    return services;
  }
}
