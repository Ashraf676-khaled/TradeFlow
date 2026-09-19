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

    group.MapPost("/register", async (
        RegisterCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      httpContext.SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresUtc);
      return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresUtc));
    });

    group.MapPost("/login", async (
        LoginCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      httpContext.SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresUtc);
      return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresUtc));
    });

    group.MapPost("/refresh", async (
        RefreshTokenRequest body, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var refreshToken = httpContext.GetRefreshTokenFromCookie();
      if (refreshToken is null)
        return Results.Unauthorized();

      var result = await sender.Send(new RefreshTokenCommand(body.AccessToken, refreshToken), ct);
      if (result.IsError)
      {
        httpContext.ClearRefreshTokenCookie(); // التوكن القديم فسد/اتلغى، امسح الكوكي
        return result.Errors.ToProblem();
      }

      httpContext.SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresUtc);
      return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresUtc));
    });

    group.MapPost("/revoke", async (ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var refreshToken = httpContext.GetRefreshTokenFromCookie();
      if (refreshToken is null)
        return Results.NoContent(); // مفيش توكن أصلًا، يبقى "متلغي" بالفعل من منظور الفرونت

      var result = await sender.Send(new RevokeTokenCommand(refreshToken), ct);
      httpContext.ClearRefreshTokenCookie();

      return result.IsSuccess ? Results.NoContent() : result.Errors.ToProblem();
    });
  }
}
