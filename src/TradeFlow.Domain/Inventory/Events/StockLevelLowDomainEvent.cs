namespace TradeFlow.Domain.Inventory.Events;

using TradeFlow.Domain.Common.Events;

public sealed class StockLevelLowDomainEvent : DomainEvent
{
  public Guid TenantId { get; }
  public Guid WarehouseId { get; }
  public Guid ProductId { get; }
  public int AvailableQuantity { get; }
  public int MinimumStock { get; }

  public StockLevelLowDomainEvent(
      Guid tenantId, Guid warehouseId, Guid productId, int availableQuantity, int minimumStock)
  {
    TenantId = tenantId;
    WarehouseId = warehouseId;
    ProductId = productId;
    AvailableQuantity = availableQuantity;
    MinimumStock = minimumStock;
  }
}
