using TradeFlow.Domain.Common.Abstractions;

namespace TradeFlow.Domain.Entities;

public class RefreshToken : Entity
{
  public Guid UserId { get; private set; }
  public string Token { get; private set; } = string.Empty;
  public DateTimeOffset ExpiresUtc { get; private set; }
  public bool IsRevoked { get; private set; }

  protected RefreshToken() { }

  public RefreshToken(Guid userId, string token, DateTimeOffset expiresUtc)
  {
    UserId = userId;
    Token = token;
    ExpiresUtc = expiresUtc;
    IsRevoked = false;
  }

  public bool IsActive => !IsRevoked && ExpiresUtc > DateTimeOffset.UtcNow;

  // سلوك الـ Rich Domain: الكلاس هو اللي بيتحكم في تغيير حالته بنفسه
  public void Revoke()
  {
    if (IsRevoked)
    {
      throw new InvalidOperationException("Refresh token is already revoked.");
    }

    IsRevoked = true;
  }
}
