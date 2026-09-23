using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationConfig
{
  public static IServiceCollection AddAuthenticationConfig(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      // قراءة الإعدادات مباشرة من الـ configuration المتاحة وقت الـ Startup (واللي بتتحدث في الـ Tests)
      var jwtSettings = configuration.GetSection("Jwt").Get<Jwt>()
                        ?? throw new InvalidOperationException("Jwt settings are missing");

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
