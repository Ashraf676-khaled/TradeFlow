namespace TradeFlow.Domain.Purchasing;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Purchasing.Events;

public sealed class PurchaseOrder : AggregateRoot, IAuditableEntity
{
  private readonly List<PurchaseOrderItem> _items = [];

  public new PurchaseOrderId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public SupplierId SupplierId { get; private set; }
  public WarehouseId WarehouseId { get; private set; }
  public DocumentNumber OrderNumber { get; private set; } = null!;
  public PurchaseOrderStatus Status { get; private set; }
  public DateTimeOffset CreatedAt { get; private set; }

  public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private PurchaseOrder() { } // EF Core

  private PurchaseOrder(
      PurchaseOrderId id, TenantId tenantId, SupplierId supplierId,
      WarehouseId warehouseId, DocumentNumber orderNumber)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    SupplierId = supplierId;
    WarehouseId = warehouseId;
    OrderNumber = orderNumber;
    Status = PurchaseOrderStatus.Draft;
    CreatedAt = DateTimeOffset.UtcNow;
  }

  public static PurchaseOrder Create(
      TenantId tenantId, SupplierId supplierId, WarehouseId warehouseId, DocumentNumber orderNumber)
      => new(PurchaseOrderId.New(), tenantId, supplierId, warehouseId, orderNumber);

  public Result<Success> AddItem(ProductId productId, Quantity quantity, Money unitCost)
  {
    if (Status != PurchaseOrderStatus.Draft)
      return PurchaseOrderErrors.NotInDraftStatus;

    var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
    if (existingItem is not null)
      return existingItem.IncreaseOrderedQuantity(quantity);

    var itemResult = PurchaseOrderItem.Create(productId, quantity, unitCost);
    if (itemResult.IsError)
      return itemResult.Errors;

    _items.Add(itemResult.Value);
    return Result.Success;
  }

  public Result<Success> Submit()
  {
    if (Status != PurchaseOrderStatus.Draft)
      return PurchaseOrderErrors.NotInDraftStatus;

    if (_items.Count == 0)
      return PurchaseOrderErrors.EmptyOrder;

    Status = PurchaseOrderStatus.Submitted;
    return Result.Success;
  }

  public Result<Success> Approve()
  {
    if (Status != PurchaseOrderStatus.Submitted)
      return PurchaseOrderErrors.NotSubmitted;

    Status = PurchaseOrderStatus.Approved;
    return Result.Success;
  }

  public Result<Success> ReceiveItem(ProductId productId, Quantity receivedQuantity)
  {
    if (Status != PurchaseOrderStatus.Approved && Status != PurchaseOrderStatus.Received)
      return PurchaseOrderErrors.NotApproved;

    var item = _items.FirstOrDefault(i => i.ProductId == productId);
    if (item is null)
      return PurchaseOrderErrors.ItemNotFound;

    var receiveResult = item.Receive(receivedQuantity);
    if (receiveResult.IsError)
      return receiveResult.Errors;

    Status = PurchaseOrderStatus.Received;

    AddDomainEvent(new PurchaseOrderReceivedDomainEvent(
        Id.Value,
        TenantId.Value,
        WarehouseId.Value,
        [new PurchaseOrderItemReceivedSnapshot(productId.Value, receivedQuantity.Value, item.UnitCost.Amount)]));

    if (_items.All(i => i.IsFullyReceived))
      Status = PurchaseOrderStatus.Completed;

    return Result.Success;
  }

  public Result<Success> Cancel()
  {
    if (Status is PurchaseOrderStatus.Completed or PurchaseOrderStatus.Received)
      return PurchaseOrderErrors.CannotCancelReceivedOrder;

    if (Status == PurchaseOrderStatus.Cancelled)
      return PurchaseOrderErrors.AlreadyCancelled;

    Status = PurchaseOrderStatus.Cancelled;
    return Result.Success;
  }
}
