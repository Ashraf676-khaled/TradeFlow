namespace TradeFlow.Application.Common.Interfaces;

public interface IHangfireJobService
{
  Task CleanupExpiredRefreshTokensAsync(CancellationToken ct);
  Task CheckLowStockLevelsAsync(CancellationToken ct);
  Task CheckOverdueInvoicesAsync(CancellationToken ct);
}
