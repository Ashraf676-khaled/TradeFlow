// Application/Common/Interfaces/IRefreshTokenService.cs
namespace TradeFlow.Application.Common.Interfaces;

public interface IRefreshTokenService
{
  Task SaveRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
  Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
  Task RevokeRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);
}
