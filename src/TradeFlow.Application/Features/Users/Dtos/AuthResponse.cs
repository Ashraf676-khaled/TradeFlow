namespace TradeFlow.Api.Endpoints.Auth;

public sealed record AuthResponse(string AccessToken, DateTimeOffset AccessTokenExpiresUtc);

public sealed record RefreshTokenRequest(string AccessToken);
