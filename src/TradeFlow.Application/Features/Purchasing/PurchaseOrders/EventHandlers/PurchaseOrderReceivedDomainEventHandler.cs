namespace TradeFlow.Application.Purchasing.PurchaseOrders.EventHandlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Purchasing.Events;

public sealed class PurchaseOrderReceivedDomainEventHandler(IApplicationDbContext context)
    : INotificationHandler<PurchaseOrderReceivedDomainEvent>
{
  public async Task Handle(PurchaseOrderReceivedDomainEvent notification, CancellationToken ct)
  {
    var warehouseId = new WarehouseId(notification.WarehouseId);
    var tenantId = new TenantId(notification.TenantId);

    foreach (var item in notification.Items)
    {
      var productId = new ProductId(item.ProductId);

      var quantityResult = Quantity.Create(item.ReceivedQuantity);   // 👈 كان item.Quantity
      if (quantityResult.IsError)
        continue;

      var stockItem = await context.StockItems
          .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);

      if (stockItem is null)
      {
        var createResult = StockItem.Create(tenantId, warehouseId, productId);
        if (createResult.IsError)
          continue;

        stockItem = createResult.Value;
        context.StockItems.Add(stockItem);
      }

      stockItem.ReceiveStock(quantityResult.Value);

      var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
      var costResult = Money.EGP(item.UnitCost);
      if (product is not null && costResult.IsSuccess)
      {
        product.ApplyPurchaseCost(costResult.Value);
      }
    }

    await context.SaveChangesAsync(ct);
  }
}
