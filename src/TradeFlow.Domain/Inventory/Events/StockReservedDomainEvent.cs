namespace TradeFlow.Domain.Inventory.Events;

using TradeFlow.Domain.Common.Events;

public sealed class StockReservedDomainEvent : DomainEvent
{
  public Guid TenantId { get; }
  public Guid WarehouseId { get; }
  public Guid ProductId { get; }
  public int Quantity { get; }

  public StockReservedDomainEvent(Guid tenantId, Guid warehouseId, Guid productId, int quantity)
  {
    TenantId = tenantId;
    WarehouseId = warehouseId;
    ProductId = productId;
    Quantity = quantity;
  }
}
