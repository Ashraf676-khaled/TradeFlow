namespace TradeFlow.Domain.Users;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class User : AggregateRoot, IAuditableEntity
{
  private readonly List<RefreshToken> _refreshTokens = [];

  public new UserId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string FullName { get; private set; } = string.Empty;
  public Email Email { get; private set; } = null!;
  public string PasswordHash { get; private set; } = string.Empty;
  public UserRole Role { get; private set; }
  public bool IsActive { get; private set; }
  public bool EmailConfirmed { get; private set; }
  public bool TwoFactorEnabled { get; private set; }
  public string? EmailOtpHash { get; private set; }
  public DateTimeOffset? EmailOtpExpiresUtc { get; private set; }
  public EmailOtpPurpose? EmailOtpPurpose { get; private set; }
  public int EmailOtpFailedAttempts { get; private set; }

  public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private User() { } // EF Core

  private User(
      UserId id,
      TenantId tenantId,
      string fullName,
      Email email,
      string passwordHash,
      UserRole role,
      bool emailConfirmed)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    FullName = fullName;
    Email = email;
    PasswordHash = passwordHash;
    Role = role;
    IsActive = true;
    EmailConfirmed = emailConfirmed;
  }

  public static Result<User> Create(
      TenantId tenantId, string fullName, Email email, string passwordHash, UserRole role)
  {
    return CreateCore(tenantId, fullName, email, passwordHash, role, true);
  }

  public static Result<User> CreateUnconfirmed(
      TenantId tenantId, string fullName, Email email, string passwordHash, UserRole role)
  {
    return CreateCore(tenantId, fullName, email, passwordHash, role, false);
  }

  public static Result<User> CreateExternal(
      TenantId tenantId, string fullName, Email email, UserRole role)
  {
    if (string.IsNullOrWhiteSpace(fullName))
      return UserErrors.FullNameRequired;

    if (fullName.Length > 150)
      return UserErrors.FullNameTooLong;

    return new User(UserId.New(), tenantId, fullName, email, string.Empty, role, true);
  }

  private static Result<User> CreateCore(
      TenantId tenantId,
      string fullName,
      Email email,
      string passwordHash,
      UserRole role,
      bool emailConfirmed)
  {
    if (string.IsNullOrWhiteSpace(fullName))
      return UserErrors.FullNameRequired;

    if (fullName.Length > 150)
      return UserErrors.FullNameTooLong;

    if (string.IsNullOrWhiteSpace(passwordHash))
      return UserErrors.PasswordHashRequired;

    return new User(UserId.New(), tenantId, fullName, email, passwordHash, role, emailConfirmed);
  }

  public void SetEmailOtp(string hash, DateTimeOffset expiresUtc, EmailOtpPurpose purpose)
  {
    EmailOtpHash = hash;
    EmailOtpExpiresUtc = expiresUtc;
    EmailOtpPurpose = purpose;
    EmailOtpFailedAttempts = 0;
  }

  public void IncrementEmailOtpFailedAttempts()
  {
    EmailOtpFailedAttempts++;
    if (EmailOtpFailedAttempts >= 5)
      ClearEmailOtp();
  }

  public void ClearEmailOtp()
  {
    EmailOtpHash = null;
    EmailOtpExpiresUtc = null;
    EmailOtpPurpose = null;
    EmailOtpFailedAttempts = 0;
  }

  public void ConfirmEmail() => EmailConfirmed = true;

  public void EnableTwoFactor() => TwoFactorEnabled = true;

  public Result<RefreshToken> IssueRefreshToken(string token, DateTimeOffset expiresUtc)
  {
    if (!IsActive)
      return UserErrors.InactiveUser;

    var tokenResult = RefreshToken.Create(Id, token, expiresUtc);
    if (tokenResult.IsError)
      return tokenResult.Errors;

    _refreshTokens.Add(tokenResult.Value);
    return tokenResult.Value;
  }

  public Result<Success> RevokeRefreshToken(string token)
  {
    var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
    if (refreshToken is null)
      return UserErrors.RefreshTokenNotFound;

    return refreshToken.Revoke();
  }

  public Result<bool> ValidateRefreshToken(string token)
  {
    var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
    if (refreshToken is null)
      return UserErrors.RefreshTokenNotFound;

    return refreshToken.IsActive;
  }

  public Result<Success> ChangePasswordHash(string newPasswordHash)
  {
    if (string.IsNullOrWhiteSpace(newPasswordHash))
      return UserErrors.PasswordHashRequired;

    PasswordHash = newPasswordHash;
    return Result.Success;
  }

  public Result<Success> ChangeRole(UserRole newRole)
  {
    Role = newRole;
    return Result.Success;
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return UserErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }

  public Result<Success> Activate()
  {
    if (IsActive)
      return UserErrors.AlreadyActive;

    IsActive = true;
    return Result.Success;
  }
}
