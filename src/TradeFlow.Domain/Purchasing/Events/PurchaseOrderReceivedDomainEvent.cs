namespace TradeFlow.Domain.Purchasing.Events;

using TradeFlow.Domain.Common.Events;

public sealed record PurchaseOrderItemReceivedSnapshot(Guid ProductId, int ReceivedQuantity, decimal UnitCost);

public sealed class PurchaseOrderReceivedDomainEvent : DomainEvent
{
  public Guid PurchaseOrderId { get; }
  public Guid TenantId { get; }
  public Guid WarehouseId { get; }
  public IReadOnlyCollection<PurchaseOrderItemReceivedSnapshot> Items { get; }

  public PurchaseOrderReceivedDomainEvent(
      Guid purchaseOrderId,
      Guid tenantId,
      Guid warehouseId,
      IReadOnlyCollection<PurchaseOrderItemReceivedSnapshot> items)
  {
    PurchaseOrderId = purchaseOrderId;
    TenantId = tenantId;
    WarehouseId = warehouseId;
    Items = items;
  }
}
