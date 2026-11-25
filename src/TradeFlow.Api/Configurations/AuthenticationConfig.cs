using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Serilog;
using TradeFlow.Application.Common.Constants;

public static class AuthenticationConfig
{
  // Reject the previously committed default if it is still supplied by a host.
  private const string RepositoryDefaultSecret = "SUPER_SECRET_KEY_EXCEEDING_32_BYTES_FOR_HS256";

  public static IServiceCollection AddAuthenticationConfig(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var jwtSettings = configuration.GetSection("Jwt").Get<Jwt>()
                      ?? throw new InvalidOperationException("Jwt settings are missing");
    if (string.IsNullOrWhiteSpace(jwtSettings.Secret)
        || Encoding.UTF8.GetByteCount(jwtSettings.Secret) < 32
        || jwtSettings.Secret.StartsWith("CHANGE_THIS_TO_", StringComparison.Ordinal)
        || string.Equals(jwtSettings.Secret, RepositoryDefaultSecret, StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
          "Jwt:Secret must be configured with a random key of at least 32 bytes using user secrets or the Jwt__Secret environment variable.");
    }

    // Log a short fingerprint (never the secret itself). After a restart the
    // fingerprint must stay identical — if it changes, Jwt__Secret is being
    // regenerated and every previously issued token/refresh cookie dies.
    var fingerprint = Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(jwtSettings.Secret)))[..12];
    Log.Information(
        "JWT signing key fingerprint: {Fingerprint}. Expiry: {ExpiryMinutes} min.",
        fingerprint,
        jwtSettings.ExpiryInMinutes);

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        // Tolerate up to 5 minutes of clock skew between the token issuer and
        // the server so tokens minted right before a restart are not rejected.
        ClockSkew = TimeSpan.FromMinutes(5),
        IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(jwtSettings.Secret))
      };
    });

    services.AddAuthorization(options =>
    {
      var platformAdminOptions = configuration.GetSection("PlatformAdmin").Get<TradeFlow.Infrastructure.Identity.PlatformAdminOptions>()
          ?? new TradeFlow.Infrastructure.Identity.PlatformAdminOptions();
      options.AddPolicy("PlatformAdmin", policy =>
          policy.RequireAuthenticatedUser()
                .RequireAssertion(context =>
                {
                  var userIdValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;
                  var userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : (Guid?)null;
                  return platformAdminOptions.IsAdmin(
                      userId,
                      context.User.FindFirst(ClaimTypes.Email)?.Value);
                }));
    });

    return services;
  }
}
