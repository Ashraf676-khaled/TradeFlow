namespace TradeFlow.Api.Endpoints.Auth;

public sealed record AuthResponse(
    string? AccessToken,
    DateTimeOffset? AccessTokenExpiresUtc,
    bool RequiresTwoFactor = false,
    string? Email = null);

public sealed record RefreshTokenRequest(string AccessToken);
