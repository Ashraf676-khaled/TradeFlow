namespace TradeFlow.Domain.Users;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;

public sealed class RefreshToken : Entity
{
    public UserId UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsActive => !IsRevoked && ExpiresUtc > DateTimeOffset.UtcNow;

    private RefreshToken() { } // EF Core

    private RefreshToken(Guid id, UserId userId, string token, DateTimeOffset expiresUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresUtc = expiresUtc;
        IsRevoked = false;
    }

    internal static Result<RefreshToken> Create(UserId userId, string token, DateTimeOffset expiresUtc)
    {
        if (string.IsNullOrWhiteSpace(token))
            return UserErrors.TokenRequired;

        if (expiresUtc <= DateTimeOffset.UtcNow)
            return UserErrors.ExpiryMustBeInFuture;

        return new RefreshToken(Guid.CreateVersion7(), userId, token, expiresUtc);
    }

    internal Result<Success> Revoke()
    {
        if (IsRevoked)
            return UserErrors.TokenAlreadyRevoked;

        IsRevoked = true;
        return Result.Success;
    }
}
