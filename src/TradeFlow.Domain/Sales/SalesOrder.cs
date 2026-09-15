namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales.Events;

public sealed class SalesOrder : AggregateRoot, IAuditableEntity
{
  private readonly List<SalesOrderItem> _items = [];

  public new SalesOrderId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public CustomerId CustomerId { get; private set; }
  public UserId SalesRepresentativeId { get; private set; }
  public WarehouseId WarehouseId { get; private set; }
  public DateTimeOffset OrderDate { get; private set; }
  public OrderStatus Status { get; private set; }
  public Discount OrderDiscount { get; private set; } = null!;

  public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private SalesOrder() { } // EF Core

  private SalesOrder(
      SalesOrderId id,
      TenantId tenantId,
      CustomerId customerId,
      UserId salesRepId,
      WarehouseId warehouseId)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    CustomerId = customerId;
    SalesRepresentativeId = salesRepId;
    WarehouseId = warehouseId;
    OrderDate = DateTimeOffset.UtcNow;
    Status = OrderStatus.Draft;
    OrderDiscount = Discount.None();
  }

  public static SalesOrder Create(
      TenantId tenantId, CustomerId customerId, UserId salesRepId, WarehouseId warehouseId)
      => new(SalesOrderId.New(), tenantId, customerId, salesRepId, warehouseId);

  public Result<Success> AddItem(ProductId productId, Quantity quantity, Money unitPrice)
  {
    if (Status != OrderStatus.Draft)
      return SalesOrderErrors.NotInDraftStatus;

    var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
    if (existingItem is not null)
      return existingItem.IncreaseQuantity(quantity);

    var itemResult = SalesOrderItem.Create(productId, quantity, unitPrice);
    if (itemResult.IsError)
      return itemResult.Errors;

    _items.Add(itemResult.Value);
    return Result.Success;
  }

  public Result<Success> RemoveItem(ProductId productId)
  {
    if (Status != OrderStatus.Draft)
      return SalesOrderErrors.NotInDraftStatus;

    var item = _items.FirstOrDefault(i => i.ProductId == productId);
    if (item is null)
      return SalesOrderErrors.ItemNotFound;

    _items.Remove(item);
    return Result.Success;
  }

  public Result<Success> ApplyItemDiscount(ProductId productId, Discount discount)
  {
    if (Status != OrderStatus.Draft)
      return SalesOrderErrors.NotInDraftStatus;

    var item = _items.FirstOrDefault(i => i.ProductId == productId);
    if (item is null)
      return SalesOrderErrors.ItemNotFound;

    return item.ApplyDiscount(discount);
  }

  public Result<Success> ApplyOrderDiscount(Discount discount)
  {
    if (Status != OrderStatus.Draft)
      return SalesOrderErrors.NotInDraftStatus;

    OrderDiscount = discount;
    return Result.Success;
  }

  public Result<Money> CalculateTotal()
  {
    var subtotal = Money.Zero();

    foreach (var item in _items)
    {
      var lineTotalResult = item.CalculateLineTotal();
      if (lineTotalResult.IsError)
        return lineTotalResult.Errors;

      var addResult = subtotal.Add(lineTotalResult.Value);
      if (addResult.IsError)
        return addResult.Errors;

      subtotal = addResult.Value;
    }

    return OrderDiscount.ApplyTo(subtotal);
  }

  public Result<Success> Confirm()
  {
    if (Status != OrderStatus.Draft)
      return SalesOrderErrors.NotInDraftStatus;

    if (_items.Count == 0)
      return SalesOrderErrors.EmptyOrder;

    Status = OrderStatus.Confirmed;

    AddDomainEvent(new SalesOrderConfirmedDomainEvent(
     Id.Value,
     TenantId.Value,
     WarehouseId.Value,
     BuildItemsSnapshot()));

    return Result.Success;
  }

  public Result<Success> Cancel()
  {
    if (Status == OrderStatus.Completed)
      return SalesOrderErrors.CannotCancelCompletedOrder;

    if (Status == OrderStatus.Cancelled)
      return SalesOrderErrors.AlreadyCancelled;

    // مهم: الحجز في المخزون بيحصل بس وقت Confirm، فالـEvent بتاعة الإلغاء
    // لازم تتبعت بس لو الطلب كان Confirmed فعلاً (يعني فيه حجز محتاج يتفك)
    var wasConfirmed = Status == OrderStatus.Confirmed;

    Status = OrderStatus.Cancelled;

    if (wasConfirmed)
    {
      AddDomainEvent(new SalesOrderCancelledDomainEvent(
      Id.Value,
      TenantId.Value,
      WarehouseId.Value,
      BuildItemsSnapshot()));
    }

    return Result.Success;
  }

  public Result<Success> Complete()
  {
    if (Status != OrderStatus.Confirmed)
      return SalesOrderErrors.NotConfirmed;

    Status = OrderStatus.Completed;
    return Result.Success;
  }

  private IReadOnlyCollection<SalesOrderItemSnapshot> BuildItemsSnapshot()
      => _items.Select(i => new SalesOrderItemSnapshot(i.ProductId.Value, i.Quantity.Value)).ToList();
}
