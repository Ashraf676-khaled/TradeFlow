using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TradeFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace TradeFlow.Infrastructure.Identity;

public class TokenProvider : ITokenProvider
{
  private readonly Jwt _jwtSettings;

  public TokenProvider(IOptions<Jwt> jwtSettings)
  {
    _jwtSettings = jwtSettings.Value;
  }

  public string GenerateAccessToken(Guid userId, IEnumerable<Claim> claims)
  {
    var allClaims = new List<Claim>(claims)
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: allClaims,
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public string GenerateRefreshToken() =>
      Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    var validationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = false,
      ValidateIssuerSigningKey = true,
      ValidIssuer = _jwtSettings.Issuer,
      ValidAudience = _jwtSettings.Audience,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
    };

    var handler = new JwtSecurityTokenHandler();
    var principal = handler.ValidateToken(token, validationParameters, out var securityToken);

    if (securityToken is not JwtSecurityToken jwtToken ||
        !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
      throw new SecurityTokenException("Invalid token");

    return principal;
  }
}
