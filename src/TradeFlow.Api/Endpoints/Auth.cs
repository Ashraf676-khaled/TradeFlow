namespace TradeFlow.Api.Endpoints.Auth;

using MediatR;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Users.Commands.Login;
using TradeFlow.Application.Users.Commands.Register;
using TradeFlow.Application.Users.Commands.RefreshToken;
using TradeFlow.Application.Users.Commands.RevokeToken;

public sealed class AuthEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/auth").WithTags("Auth");

    group.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess
          ? Results.Ok(result.Value)
          : result.Errors.ToProblem();
    });

    group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess
          ? Results.Ok(result.Value)
          : result.Errors.ToProblem();
    });

    group.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess
          ? Results.Ok(result.Value)
          : result.Errors.ToProblem();
    });

    group.MapPost("/revoke", async (RevokeTokenCommand command, ISender sender, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      return result.IsSuccess
          ? Results.NoContent()
          : result.Errors.ToProblem();
    });
  }
}
