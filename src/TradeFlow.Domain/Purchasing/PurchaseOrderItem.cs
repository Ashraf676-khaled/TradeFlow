namespace TradeFlow.Domain.Purchasing;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class PurchaseOrderItem : Entity
{
  public ProductId ProductId { get; private set; }
  public Quantity OrderedQuantity { get; private set; } = null!;
  public Quantity ReceivedQuantity { get; private set; } = null!;
  public Money UnitCost { get; private set; } = null!;

  public bool IsFullyReceived => ReceivedQuantity.IsGreaterThanOrEqual(OrderedQuantity);

  private PurchaseOrderItem() { } // EF Core

  private PurchaseOrderItem(Guid id, ProductId productId, Quantity orderedQuantity, Money unitCost)
      : base(id)
  {
    ProductId = productId;
    OrderedQuantity = orderedQuantity;
    UnitCost = unitCost;
    ReceivedQuantity = Quantity.Zero();
  }

  internal static Result<PurchaseOrderItem> Create(ProductId productId, Quantity orderedQuantity, Money unitCost)
  {
    if (unitCost.Amount <= 0)
      return PurchaseOrderErrors.InvalidUnitCost;

    return new PurchaseOrderItem(Guid.CreateVersion7(), productId, orderedQuantity, unitCost);
  }

  internal Result<Success> IncreaseOrderedQuantity(Quantity additional)
  {
    var result = OrderedQuantity.Add(additional);
    if (result.IsError)
      return result.Errors;

    OrderedQuantity = result.Value;
    return Result.Success;
  }

  internal Result<Success> Receive(Quantity quantity)
  {
    var newReceivedResult = ReceivedQuantity.Add(quantity);
    if (newReceivedResult.IsError)
      return newReceivedResult.Errors;

    if (newReceivedResult.Value.IsGreaterThan(OrderedQuantity))
      return PurchaseOrderErrors.ReceivedExceedsOrdered;

    ReceivedQuantity = newReceivedResult.Value;
    return Result.Success;
  }
}
