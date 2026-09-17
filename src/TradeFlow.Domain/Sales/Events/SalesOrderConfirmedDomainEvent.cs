namespace TradeFlow.Domain.Sales.Events;

using TradeFlow.Domain.Common.Events;

public sealed record SalesOrderItemSnapshot(Guid ProductId, int Quantity);

public sealed class SalesOrderConfirmedDomainEvent : DomainEvent
{
  public Guid SalesOrderId { get; }
  public Guid TenantId { get; }
  public Guid WarehouseId { get; }
  public IReadOnlyCollection<SalesOrderItemSnapshot> Items { get; }

  public SalesOrderConfirmedDomainEvent(
      Guid salesOrderId,
      Guid tenantId,
      Guid warehouseId,
      IReadOnlyCollection<SalesOrderItemSnapshot> items)
  {
    SalesOrderId = salesOrderId;
    TenantId = tenantId;
    WarehouseId = warehouseId;
    Items = items;
  }
}
