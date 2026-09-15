namespace TradeFlow.Domain.Inventory.Events;

using TradeFlow.Domain.Common.Events;

public sealed class StockReleasedDomainEvent : DomainEvent
{
  public Guid TenantId { get; }
  public Guid WarehouseId { get; }
  public Guid ProductId { get; }
  public int Quantity { get; }

  public StockReleasedDomainEvent(Guid tenantId, Guid warehouseId, Guid productId, int quantity)
  {
    TenantId = tenantId;
    WarehouseId = warehouseId;
    ProductId = productId;
    Quantity = quantity;
  }
}
