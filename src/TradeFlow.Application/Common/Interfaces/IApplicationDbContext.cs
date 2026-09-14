using TradeFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TradeFlow.Appliction.Common.Interfaces;

public interface IApplicationDbContext
{
  DbSet<RefreshToken> RefreshTokens { get; }
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
