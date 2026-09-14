// Application/Common/Interfaces/ITokenProvider.cs
using System.Security.Claims;

namespace TradeFlow.Application.Common.Interfaces;

public interface ITokenProvider
{
  string GenerateAccessToken(Guid userId, IEnumerable<Claim> claims);
  string GenerateRefreshToken();
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
