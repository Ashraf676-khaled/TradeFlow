namespace TradeFlow.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Tenants;
using TradeFlow.Domain.Users;

public class IdentityService : IIdentityService
{
  private readonly IUserRepository _userRepository;
  private readonly IApplicationDbContext _context;
  private readonly ITokenProvider _tokenProvider;
  private readonly Jwt _jwtSettings;
  private readonly PasswordHasher<User> _passwordHasher = new();

  private const int RefreshTokenDays = 7;

  public IdentityService(
      IUserRepository userRepository,
      IApplicationDbContext context,
      ITokenProvider tokenProvider,
      IOptions<Jwt> jwtSettings)
  {
    _userRepository = userRepository;
    _context = context;
    _tokenProvider = tokenProvider;
    _jwtSettings = jwtSettings.Value;
  }

  public async Task<Result<AuthResult>> RegisterAsync(
    string companyName, string fullName, string email, string password, CancellationToken ct = default)
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

    var tenant = tenantResult.Value;

    // أول مستخدم في أي Tenant جديد لازم يبقى Admin
    var userResult = User.Create(tenant.Id, fullName, emailResult.Value, "temp", UserRole.Admin);
    if (userResult.IsError)
      return userResult.Errors;

    var user = userResult.Value;
    var hashedPassword = _passwordHasher.HashPassword(user, password);

    var changeHashResult = user.ChangePasswordHash(hashedPassword);
    if (changeHashResult.IsError)
      return changeHashResult.Errors;

    _context.Tenants.Add(tenant);
    _userRepository.Add(user);
    await _context.SaveChangesAsync(ct);

    return await IssueTokensAsync(user, ct);
  }
  public async Task<Result<AuthResult>> LoginAsync(string email, string password, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByEmailAsync(email, ct);
    if (user is null)
      return UserErrors.NotFound;

    if (!user.IsActive)
      return UserErrors.InactiveUser;

    var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
    if (verifyResult == PasswordVerificationResult.Failed)
      return UserErrors.NotFound; // عمدًا مش بنقول "الباسورد غلط" منفصلة، عشان مانسربش هل الإيميل موجود

    // لو المكتبة قالت إن الـ Hash محتاج تحديث لخوارزمية أحدث، نحدثه بصمت
    if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
    {
      var rehashed = _passwordHasher.HashPassword(user, password);
      user.ChangePasswordHash(rehashed);
      await _context.SaveChangesAsync(ct);
    }

    return await IssueTokensAsync(user, ct);
  }

  public async Task<Result<AuthResult>> RefreshAsync(
      string accessToken, string refreshToken, CancellationToken ct = default)
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

  public async Task<Result<Success>> RevokeAsync(string refreshToken, CancellationToken ct = default)
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

  private async Task<Result<AuthResult>> IssueTokensAsync(User user, CancellationToken ct)
  {
    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
        new(System.Security.Claims.ClaimTypes.Email, user.Email.Value),
        new(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
        new(TradeFlow.Application.Common.Constants.AppClaimTypes.TenantId, user.TenantId.Value.ToString()) // 👈 جديد
    };

    var accessToken = _tokenProvider.GenerateAccessToken(user.Id.Value, claims);
    var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
    var expiresUtc = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays);

    var issueResult = user.IssueRefreshToken(refreshTokenValue, expiresUtc);
    if (issueResult.IsError)
      return issueResult.Errors;

    await _context.SaveChangesAsync(ct);

    var accessTokenExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);
    return new AuthResult(accessToken, refreshTokenValue, accessTokenExpiresUtc);
  }
}
