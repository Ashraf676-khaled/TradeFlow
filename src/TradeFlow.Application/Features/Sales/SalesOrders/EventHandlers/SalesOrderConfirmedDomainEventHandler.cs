namespace TradeFlow.Application.Sales.SalesOrders.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales.Events;

public sealed class SalesOrderConfirmedDomainEventHandler(IApplicationDbContext context)
    : INotificationHandler<SalesOrderConfirmedDomainEvent>
{
  public async Task Handle(SalesOrderConfirmedDomainEvent notification, CancellationToken ct)
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

      var reserveResult = stockItem.Reserve(quantityResult.Value);
      if (reserveResult.IsError)
        throw new DbUpdateConcurrencyException(
            $"Insufficient stock to reserve for product {item.ProductId}: {reserveResult.TopError.Description}");
    }

    // No SaveChanges here: everything is persisted by the original SaveChanges
    // call that raised this event (single atomic transaction).
  }
}
