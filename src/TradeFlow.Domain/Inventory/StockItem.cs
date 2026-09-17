namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class StockItem : AggregateRoot, IAuditableEntity
{
  public new StockItemId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public WarehouseId WarehouseId { get; private set; }
  public ProductId ProductId { get; private set; }
  public Quantity AvailableQuantity { get; private set; } = null!;
  public Quantity ReservedQuantity { get; private set; } = null!;

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private StockItem() { } // EF Core

  private StockItem(StockItemId id, TenantId tenantId, WarehouseId warehouseId, ProductId productId)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    WarehouseId = warehouseId;
    ProductId = productId;
    AvailableQuantity = Quantity.Zero();
    ReservedQuantity = Quantity.Zero();
  }

  public static Result<StockItem> Create(TenantId tenantId, WarehouseId warehouseId, ProductId productId)
      => new StockItem(StockItemId.New(), tenantId, warehouseId, productId);

  public Result<Success> ReceiveStock(Quantity quantity)
  {
    var addResult = AvailableQuantity.Add(quantity);
    if (addResult.IsError)
      return addResult.Errors;

    AvailableQuantity = addResult.Value;
    return Result.Success;
  }

  public Result<Success> Reserve(Quantity quantity)
  {
    if (!AvailableQuantity.IsGreaterThanOrEqual(quantity))
      return StockItemErrors.InsufficientStock;

    var newAvailable = AvailableQuantity.Subtract(quantity);
    if (newAvailable.IsError)
      return newAvailable.Errors;

    var newReserved = ReservedQuantity.Add(quantity);
    if (newReserved.IsError)
      return newReserved.Errors;

    AvailableQuantity = newAvailable.Value;
    ReservedQuantity = newReserved.Value;
    return Result.Success;
  }

  public Result<Success> ReleaseReservation(Quantity quantity)
  {
    if (!ReservedQuantity.IsGreaterThanOrEqual(quantity))
      return StockItemErrors.CannotReleaseMoreThanReserved;

    var newReserved = ReservedQuantity.Subtract(quantity);
    if (newReserved.IsError)
      return newReserved.Errors;

    var newAvailable = AvailableQuantity.Add(quantity);
    if (newAvailable.IsError)
      return newAvailable.Errors;

    ReservedQuantity = newReserved.Value;
    AvailableQuantity = newAvailable.Value;
    return Result.Success;
  }

  /// <summary>
  /// البضاعة خرجت فعلياً من المخزن (اتشحنت للعميل). بيتنادى بعد ما الـSalesOrder يكمل،
  /// مش وقت الـReserve. الكمية دي كانت اتخصمت من الـAvailable أصلاً وقت الـReserve،
  /// فهنا بس بنشيلها من الـReserved لأنها بقت خارج النظام نهائياً.
  /// </summary>
  public Result<Success> ConfirmDeduction(Quantity quantity)
  {
    if (!ReservedQuantity.IsGreaterThanOrEqual(quantity))
      return StockItemErrors.CannotDeductMoreThanReserved;

    var newReserved = ReservedQuantity.Subtract(quantity);
    if (newReserved.IsError)
      return newReserved.Errors;

    ReservedQuantity = newReserved.Value;
    return Result.Success;
  }

  public Result<Success> TransferOut(Quantity quantity)
  {
    if (!AvailableQuantity.IsGreaterThanOrEqual(quantity))
      return StockItemErrors.InsufficientStock;

    var newAvailable = AvailableQuantity.Subtract(quantity);
    if (newAvailable.IsError)
      return newAvailable.Errors;

    AvailableQuantity = newAvailable.Value;
    return Result.Success;
  }
}
