namespace TradeFlow.Application.Sales.SalesOrders.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales.Events;

public sealed class SalesOrderCancelledDomainEventHandler(IApplicationDbContext context)
    : INotificationHandler<SalesOrderCancelledDomainEvent>
{
  public async Task Handle(SalesOrderCancelledDomainEvent notification, CancellationToken ct)
  {
    var warehouseId = new WarehouseId(notification.WarehouseId);

    foreach (var item in notification.Items)
    {
      var productId = new ProductId(item.ProductId);
      var quantityResult = Quantity.Create(item.Quantity);
      if (quantityResult.IsError)
        continue;

      var stockItem = await context.StockItems
          .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);

      if (stockItem is null)
        throw new DbUpdateConcurrencyException(
            $"No stock record for product {item.ProductId} in warehouse {notification.WarehouseId}.");

      var releaseResult = stockItem.ReleaseReservation(quantityResult.Value);
      if (releaseResult.IsError)
        throw new DbUpdateConcurrencyException(
            $"Unable to release reservation for product {item.ProductId}: {releaseResult.TopError.Description}");
    }

    // No SaveChanges here: the release is persisted by the original SaveChanges
    // call that raised this event — avoids re-entrant SaveChanges inside the interceptor.
  }
}
