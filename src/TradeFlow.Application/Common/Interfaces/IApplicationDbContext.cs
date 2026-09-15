using Microsoft.EntityFrameworkCore;
using TradeFlow.Domain.Users;

namespace TradeFlow.Appliction.Common.Interfaces;

public interface IApplicationDbContext
{
  DbSet<RefreshToken> RefreshTokens { get; }
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
