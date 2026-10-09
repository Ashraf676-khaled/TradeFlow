using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationConfig
{
  public static IServiceCollection AddAuthenticationConfig(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var jwtSettings = configuration.GetSection("Jwt").Get<Jwt>()
                      ?? throw new InvalidOperationException("Jwt settings are missing");
    if (string.IsNullOrWhiteSpace(jwtSettings.Secret)
        || Encoding.UTF8.GetByteCount(jwtSettings.Secret) < 32
        || jwtSettings.Secret.StartsWith("CHANGE_THIS_TO_", StringComparison.Ordinal))
    {
      throw new InvalidOperationException(
          "Jwt:Secret must be configured with a random key of at least 32 bytes using user secrets or the Jwt__Secret environment variable.");
    }

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
        IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(jwtSettings.Secret))
      };
    });

    services.AddAuthorization();

    return services;
  }
}
