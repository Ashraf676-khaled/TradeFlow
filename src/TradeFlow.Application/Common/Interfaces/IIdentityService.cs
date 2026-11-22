namespace TradeFlow.Application.Common.Interfaces;

using TradeFlow.Domain.Common.Results;

public interface IIdentityService
{
  Task<Result<AuthResult>> RegisterAsync(string companyName, string fullName, string email, string password, CancellationToken ct = default);
  Task<Result<AuthResult>> LoginAsync(string email, string password, CancellationToken ct = default);
  Task<Result<AuthResult>> VerifyOtpAsync(string email, string code, CancellationToken ct = default);
  Task<Result<AuthResult>> GoogleLoginAsync(string idToken, CancellationToken ct = default);
  Task<Result<AuthResult>> EnableTwoFactorAsync(string email, CancellationToken ct = default);
  Task<Result<AuthResult>> RefreshAsync(string accessToken, string refreshToken, CancellationToken ct = default);
  Task<Result<Success>> RevokeAsync(string refreshToken, CancellationToken ct = default);
}

public sealed record AuthResult(
    string? AccessToken,
    string? RefreshToken,
    DateTimeOffset? AccessTokenExpiresUtc,
    DateTimeOffset? RefreshTokenExpiresUtc,
    bool RequiresTwoFactor = false,
    string? Email = null);
