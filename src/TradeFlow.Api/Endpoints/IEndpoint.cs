// Api/Endpoints/IEndpoint.cs
namespace TradeFlow.Api.Endpoints;

public interface IEndpoint
{
  void MapEndpoint(IEndpointRouteBuilder app);
}
