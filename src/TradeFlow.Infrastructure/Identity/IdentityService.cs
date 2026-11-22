namespace TradeFlow.Infrastructure.Identity;

using System.Globalization;
using System.Security.Cryptography;
using Google.Apis.Auth;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Tenants;
using TradeFlow.Domain.Users;
using TradeFlow.Infrastructure.Settings;

public class IdentityService : IIdentityService
{
  private readonly IUserRepository _userRepository;
  private readonly IApplicationDbContext _context;
  private readonly ITokenProvider _tokenProvider;
  private readonly IBackgroundJobClient _backgroundJobClient;
  private readonly Jwt _jwtSettings;
  private readonly GoogleAuthSettings _googleAuthSettings;
  private readonly PlatformAdminOptions _platformAdminOptions;
  private readonly PasswordHasher<User> _passwordHasher = new();

  private const int RefreshTokenDays = 7;
  private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

  public IdentityService(
      IUserRepository userRepository,
      IApplicationDbContext context,
      ITokenProvider tokenProvider,
      IBackgroundJobClient backgroundJobClient,
      IOptions<Jwt> jwtSettings,
      IOptions<GoogleAuthSettings> googleAuthSettings,
      IOptions<PlatformAdminOptions> platformAdminOptions)
  {
    _userRepository = userRepository;
    _context = context;
    _tokenProvider = tokenProvider;
    _backgroundJobClient = backgroundJobClient;
    _jwtSettings = jwtSettings.Value;
    _googleAuthSettings = googleAuthSettings.Value;
    _platformAdminOptions = platformAdminOptions.Value;
  }

  public async Task<Result<AuthResult>> RegisterAsync(
      string companyName,
      string fullName,
      string email,
      string password,
      CancellationToken ct = default)
  {
    var emailResult = Email.Create(email);
    if (emailResult.IsError)
      return emailResult.Errors;

    var existing = await _userRepository.GetByEmailAsync(emailResult.Value.Value, ct);
    if (existing is not null)
      return UserErrors.EmailAlreadyExists;

    var tenantResult = Tenant.Create(companyName);
    if (tenantResult.IsError)
      return tenantResult.Errors;

    var userResult = User.CreateUnconfirmed(
        tenantResult.Value.Id,
        fullName,
        emailResult.Value,
        "temp",
        UserRole.Admin);
    if (userResult.IsError)
      return userResult.Errors;

    var user = userResult.Value;
    var hashedPassword = _passwordHasher.HashPassword(user, password);
    var changeHashResult = user.ChangePasswordHash(hashedPassword);
    if (changeHashResult.IsError)
      return changeHashResult.Errors;

    _context.Tenants.Add(tenantResult.Value);
    _userRepository.Add(user);
    return await IssueOtpChallengeAsync(user, EmailOtpPurpose.ConfirmEmail, ct);
  }

  public async Task<Result<AuthResult>> LoginAsync(
      string email,
      string password,
      CancellationToken ct = default)
  {
    var user = await _userRepository.GetByEmailAsync(email, ct);
    if (user is null)
      return UserErrors.NotFound;
    if (!user.IsActive)
      return UserErrors.InactiveUser;
    if (string.IsNullOrEmpty(user.PasswordHash))
      return UserErrors.NotFound;

    var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
    if (verifyResult == PasswordVerificationResult.Failed)
      return UserErrors.NotFound;

    if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
    {
      var rehashed = _passwordHasher.HashPassword(user, password);
      user.ChangePasswordHash(rehashed);
      await _context.SaveChangesAsync(ct);
    }

    if (!user.EmailConfirmed)
      return await IssueOtpChallengeAsync(user, EmailOtpPurpose.ConfirmEmail, ct);

    if (user.TwoFactorEnabled)
      return await IssueOtpChallengeAsync(user, EmailOtpPurpose.TwoFactor, ct);

    return await IssueTokensAsync(user, ct);
  }

  public async Task<Result<AuthResult>> VerifyOtpAsync(
      string email,
      string code,
      CancellationToken ct = default)
  {
    var user = await _userRepository.GetByEmailAsync(email, ct);
    if (user is null || !user.IsActive || string.IsNullOrEmpty(user.EmailOtpHash))
      return UserErrors.InvalidEmailOtp;

    if (user.EmailOtpExpiresUtc <= DateTimeOffset.UtcNow || user.EmailOtpFailedAttempts >= 5)
    {
      user.ClearEmailOtp();
      await _context.SaveChangesAsync(ct);
      return UserErrors.InvalidEmailOtp;
    }

    if (_passwordHasher.VerifyHashedPassword(user, user.EmailOtpHash, code)
        == PasswordVerificationResult.Failed)
    {
      user.IncrementEmailOtpFailedAttempts();
      await _context.SaveChangesAsync(ct);
      return UserErrors.InvalidEmailOtp;
    }

    var purpose = user.EmailOtpPurpose;
    user.ClearEmailOtp();

    if (purpose == EmailOtpPurpose.ConfirmEmail)
      user.ConfirmEmail();
    else if (purpose == EmailOtpPurpose.EnableTwoFactor)
      user.EnableTwoFactor();
    else if (purpose != EmailOtpPurpose.TwoFactor)
      return UserErrors.InvalidEmailOtp;

    return await IssueTokensAsync(user, ct);
  }

  public async Task<Result<AuthResult>> EnableTwoFactorAsync(
      string email,
      CancellationToken ct = default)
  {
    var user = await _userRepository.GetByEmailAsync(email, ct);
    if (user is null || !user.IsActive)
      return UserErrors.NotFound;

    if (!user.EmailConfirmed)
      return UserErrors.EmailNotConfirmed;

    return await IssueOtpChallengeAsync(user, EmailOtpPurpose.EnableTwoFactor, ct);
  }

  public async Task<Result<AuthResult>> GoogleLoginAsync(
      string idToken,
      CancellationToken ct = default)
  {
    if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
      throw new InvalidOperationException("GoogleAuth:ClientId must be configured.");

    GoogleJsonWebSignature.Payload payload;
    try
    {
      payload = await GoogleJsonWebSignature.ValidateAsync(
          idToken,
          new GoogleJsonWebSignature.ValidationSettings
          {
            Audience = new[] { _googleAuthSettings.ClientId }
          });
    }
    catch (InvalidJwtException)
    {
      return UserErrors.GoogleLoginFailed;
    }

    if (string.IsNullOrWhiteSpace(payload.Subject)
        || string.IsNullOrWhiteSpace(payload.Email)
        || payload.EmailVerified != true)
      return UserErrors.GoogleLoginFailed;

    var emailResult = Email.Create(payload.Email);
    if (emailResult.IsError)
      return UserErrors.GoogleLoginFailed;

    var externalLogin = await _context.ExternalLogins
        .SingleOrDefaultAsync(
            login => login.Provider == "Google" && login.ProviderSubject == payload.Subject,
            ct);
    User? user = null;

    if (externalLogin is not null)
      user = await _userRepository.GetByIdAsync(externalLogin.UserId, ct);

    if (externalLogin is not null && user is null)
      return UserErrors.GoogleLoginFailed;

    if (user is null)
      user = await _userRepository.GetByEmailAsync(emailResult.Value.Value, ct);

    if (user is null)
    {
      var normalizedEmail = emailResult.Value.Value;
      var emailPrefixLength = normalizedEmail.IndexOf('@');
      var fullName = string.IsNullOrWhiteSpace(payload.Name)
          ? normalizedEmail[..emailPrefixLength]
          : payload.Name.Trim();
      var tenantResult = Tenant.Create($"{fullName}'s Workspace");
      if (tenantResult.IsError)
        return tenantResult.Errors;

      var userResult = User.CreateExternal(
          tenantResult.Value.Id,
          fullName,
          emailResult.Value,
          UserRole.Admin);
      if (userResult.IsError)
        return userResult.Errors;

      user = userResult.Value;
      _context.Tenants.Add(tenantResult.Value);
      _userRepository.Add(user);
    }
    else if (!user.EmailConfirmed)
    {
      user.ConfirmEmail();
    }

    if (!user.IsActive)
      return UserErrors.InactiveUser;

    if (externalLogin is null)
      _context.ExternalLogins.Add(new ExternalLogin(user.Id, "Google", payload.Subject));

    await _context.SaveChangesAsync(ct);

    return await IssueTokensAsync(user, ct);
  }

  public async Task<Result<AuthResult>> RefreshAsync(
      string accessToken,
      string refreshToken,
      CancellationToken ct = default)
  {
    var principal = _tokenProvider.GetPrincipalFromExpiredToken(accessToken);
    var userIdClaim = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? principal?.FindFirst("sub")?.Value;

    if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userIdGuid))
      return UserErrors.NotFound;

    var user = await _userRepository.GetByIdAsync(new UserId(userIdGuid), ct);
    if (user is null)
      return UserErrors.NotFound;

    var validateResult = user.ValidateRefreshToken(refreshToken);
    if (validateResult.IsError || !validateResult.Value)
      return UserErrors.RefreshTokenNotFound;

    var revokeResult = user.RevokeRefreshToken(refreshToken);
    if (revokeResult.IsError)
      return revokeResult.Errors;

    await _context.SaveChangesAsync(ct);
    return await IssueTokensAsync(user, ct);
  }

  public async Task<Result<Success>> RevokeAsync(
      string refreshToken,
      CancellationToken ct = default)
  {
    var tokenEntity = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

    if (tokenEntity is null)
      return UserErrors.RefreshTokenNotFound;

    var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, ct);
    if (user is null)
      return UserErrors.NotFound;

    var result = user.RevokeRefreshToken(refreshToken);
    if (result.IsError)
      return result.Errors;

    await _context.SaveChangesAsync(ct);
    return Result.Success;
  }

  private async Task<Result<AuthResult>> IssueOtpChallengeAsync(
      User user,
      EmailOtpPurpose purpose,
      CancellationToken ct)
  {
    var code = RandomNumberGenerator.GetInt32(0, 1_000_000)
        .ToString("D6", CultureInfo.InvariantCulture);
    var codeHash = _passwordHasher.HashPassword(user, code);
    user.SetEmailOtp(codeHash, DateTimeOffset.UtcNow.Add(OtpLifetime), purpose);
    await _context.SaveChangesAsync(ct);

    var subject = purpose switch
    {
      EmailOtpPurpose.ConfirmEmail => "Confirm your TradeFlow email",
      EmailOtpPurpose.EnableTwoFactor => "Enable TradeFlow two-factor authentication",
      _ => "Your TradeFlow verification code"
    };
    var body = $"Your TradeFlow verification code is {code}. It expires in 5 minutes.";
    _backgroundJobClient.Enqueue<IEmailService>(
        service => service.SendEmailAsync(user.Email.Value, subject, body, CancellationToken.None));

    return new AuthResult(null, null, null, null, true, user.Email.Value);
  }

  private async Task<Result<AuthResult>> IssueTokensAsync(User user, CancellationToken ct)
  {
    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
        new(System.Security.Claims.ClaimTypes.Email, user.Email.Value),
        new(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
        new(TradeFlow.Application.Common.Constants.AppClaimTypes.TenantId, user.TenantId.Value.ToString())
    };
    if (_platformAdminOptions.IsAdmin(user.Id.Value, user.Email.Value))
      claims.Add(new(
          TradeFlow.Application.Common.Constants.AppClaimTypes.PlatformAdmin,
          "true"));

    var accessToken = _tokenProvider.GenerateAccessToken(user.Id.Value, claims);
    var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
    var refreshTokenExpiresUtc = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays);

    var issueResult = user.IssueRefreshToken(refreshTokenValue, refreshTokenExpiresUtc);
    if (issueResult.IsError)
      return issueResult.Errors;

    await _context.SaveChangesAsync(ct);

    var accessTokenExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);
    return new AuthResult(
        accessToken,
        refreshTokenValue,
        accessTokenExpiresUtc,
        refreshTokenExpiresUtc,
        false,
        user.Email.Value);
  }
}
