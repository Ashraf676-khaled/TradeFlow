namespace TradeFlow.Infrastructure.Identity;

using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Appliction.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;

public class RefreshTokenService : IRefreshTokenService
{
  private readonly IApplicationDbContext _context;
  private readonly IUserRepository _userRepository;

  public RefreshTokenService(IApplicationDbContext context, IUserRepository userRepository)
  {
    _context = context;
    _userRepository = userRepository;
  }

  public async Task SaveRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByIdAsync(new UserId(userId), ct);
    if (user is null) return;

    var result = user.IssueRefreshToken(token, DateTimeOffset.UtcNow.AddDays(7));
    if (result.IsError) return;

    await _context.SaveChangesAsync(ct);
  }

  public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
      => await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(
          _context.RefreshTokens,
          r => r.UserId == new UserId(userId)
               && r.Token == token
               && !r.IsRevoked
               && r.ExpiresUtc > DateTimeOffset.UtcNow,
          ct);

  public async Task RevokeRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByIdAsync(new UserId(userId), ct);
    if (user is null) return;

    var result = user.RevokeRefreshToken(token);
    if (result.IsError) return;

    await _context.SaveChangesAsync(ct);
  }
}
