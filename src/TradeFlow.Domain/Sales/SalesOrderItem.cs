namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class SalesOrderItem : Entity
{
  public ProductId ProductId { get; private set; }
  public Quantity Quantity { get; private set; } = null!;
  public Money UnitPrice { get; private set; } = null!;
  public Discount ItemDiscount { get; private set; } = null!;

  private SalesOrderItem() { } // EF Core

  private SalesOrderItem(Guid id, ProductId productId, Quantity quantity, Money unitPrice)
      : base(id)
  {
    ProductId = productId;
    Quantity = quantity;
    UnitPrice = unitPrice;
    ItemDiscount = Discount.None();
  }

  internal static Result<SalesOrderItem> Create(ProductId productId, Quantity quantity, Money unitPrice)
  {
    if (unitPrice.Amount <= 0)
      return SalesOrderErrors.InvalidUnitPrice;

    return new SalesOrderItem(Guid.CreateVersion7(), productId, quantity, unitPrice);
  }

  internal Result<Success> IncreaseQuantity(Quantity additional)
  {
    var result = Quantity.Add(additional);
    if (result.IsError)
      return result.Errors;

    Quantity = result.Value;
    return Result.Success;
  }

  internal Result<Success> ApplyDiscount(Discount discount)
  {
    ItemDiscount = discount;
    return Result.Success;
  }

  internal Result<Money> CalculateLineTotal()
  {
    var grossResult = UnitPrice.Multiply(Quantity.Value);
    if (grossResult.IsError)
      return grossResult.Errors;

    return ItemDiscount.ApplyTo(grossResult.Value);
  }
}
