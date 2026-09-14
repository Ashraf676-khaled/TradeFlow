// Api/Extensions/WebApplicationExtensions.cs
using TradeFlow.Api.Endpoints;

namespace TradeFlow.Api.Extensions;

public static class WebApplicationExtensions
{
  public static void MapEndpoints(this WebApplication app)
  {
    var endpointTypes = typeof(IAssemblyMarker).Assembly
        .GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

    foreach (var type in endpointTypes)
    {
      var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
      endpoint.MapEndpoint(app);
    }
  }
}
