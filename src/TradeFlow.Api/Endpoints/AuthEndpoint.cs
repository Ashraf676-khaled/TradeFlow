namespace TradeFlow.Api.Endpoints.Auth;

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using TradeFlow.Api.Extensions;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Users.Commands.EnableTwoFactor;
using TradeFlow.Application.Users.Commands.GoogleLogin;
using TradeFlow.Application.Users.Commands.Login;
using TradeFlow.Application.Users.Commands.Register;
using TradeFlow.Application.Users.Commands.RefreshToken;
using TradeFlow.Application.Users.Commands.RevokeToken;
using TradeFlow.Application.Users.Commands.VerifyOtp;

public sealed class AuthEndpoints : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/auth")
        .WithTags("Auth")
        .RequireRateLimiting("auth");

    group.MapPost("/register", async (
        RegisterCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      return CreateAuthResponse(result.Value, httpContext);
    });

    group.MapPost("/login", async (
        LoginCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      return CreateAuthResponse(result.Value, httpContext);
    });

    group.MapPost("/verify-otp", async (
        VerifyOtpCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      return CreateAuthResponse(result.Value, httpContext);
    });

    group.MapPost("/google-login", async (
        GoogleLoginCommand command, ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var result = await sender.Send(command, ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      return CreateAuthResponse(result.Value, httpContext);
    });

    group.MapPost("/enable-two-factor", async (
        ISender sender, HttpContext httpContext, CancellationToken ct) =>
    {
      var email = httpContext.User.FindFirstValue(ClaimTypes.Email);
      if (string.IsNullOrWhiteSpace(email))
        return Results.Unauthorized();

      var result = await sender.Send(new EnableTwoFactorCommand(email), ct);
      if (result.IsError)
        return result.Errors.ToProblem();

      return CreateAuthResponse(result.Value, httpContext);
    }).RequireAuthorization();

    // Refresh uses its own bucket (30/min) so concurrent tabs refreshing
    // silently never trip the stricter 10/min login limit and get a 429.
    group.MapPost("/refresh", Refresh).RequireRateLimiting("auth-refresh");
    group.MapPost("/refresh-token", Refresh).RequireRateLimiting("auth-refresh");

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

  private static async Task<IResult> Refresh(
      RefreshTokenRequest body, ISender sender, HttpContext httpContext, CancellationToken ct)
  {
    var refreshToken = httpContext.GetRefreshTokenFromCookie();
    if (refreshToken is null)
      return Results.Unauthorized();

    var result = await sender.Send(new RefreshTokenCommand(body.AccessToken, refreshToken), ct);
    if (result.IsError)
    {
      httpContext.ClearRefreshTokenCookie();
      return result.Errors.ToProblem();
    }

    return CreateAuthResponse(result.Value, httpContext);
  }

  private static IResult CreateAuthResponse(AuthResult result, HttpContext httpContext)
  {
    if (result.RequiresTwoFactor)
      return Results.Ok(new AuthResponse(null, null, true, result.Email));

    if (result.AccessToken is null
        || result.RefreshToken is null
        || result.AccessTokenExpiresUtc is null
        || result.RefreshTokenExpiresUtc is null)
      throw new InvalidOperationException("The authentication result does not contain a complete token set.");

    httpContext.SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresUtc.Value);
    return Results.Ok(new AuthResponse(result.AccessToken, result.AccessTokenExpiresUtc, false, result.Email));
  }
}
