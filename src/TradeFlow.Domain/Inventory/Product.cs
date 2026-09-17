namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Product : AggregateRoot, IAuditableEntity
{
  public new ProductId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public string Name { get; private set; } = string.Empty;
  public Sku Sku { get; private set; } = null!;
  public Money SellingPrice { get; private set; } = null!;
  public Money Cost { get; private set; } = null!;
  public int MinimumStock { get; private set; }
  public bool IsActive { get; private set; }

  // Explicit Implementation — مش ظاهرة على product.CreatedAtUtc مباشرة
  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Product() { } // EF Core

  private Product(
      ProductId id,
      TenantId tenantId,
      string name,
      Sku sku,
      Money sellingPrice,
      Money initialCost,
      int minimumStock)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    Name = name;
    Sku = sku;
    SellingPrice = sellingPrice;
    Cost = initialCost;
    MinimumStock = minimumStock;
    IsActive = true;
  }

  public static Result<Product> Create(
      TenantId tenantId,
      string name,
      Sku sku,
      Money sellingPrice,
      Money initialCost,
      int minimumStock)
  {
    if (string.IsNullOrWhiteSpace(name))
      return ProductErrors.NameRequired;

    if (name.Length > 200)
      return ProductErrors.NameTooLong;

    if (sellingPrice.Amount <= 0)
      return ProductErrors.InvalidSellingPrice;

    if (minimumStock < 0)
      return ProductErrors.InvalidMinimumStock;

    return new Product(ProductId.New(), tenantId, name, sku, sellingPrice, initialCost, minimumStock);
  }

  public Result<Success> ChangeSellingPrice(Money newPrice)
  {
    if (newPrice.Amount <= 0)
      return ProductErrors.InvalidSellingPrice;

    SellingPrice = newPrice;
    return Result.Success;
  }

  /// <summary>
  /// يتنادى فقط من الـPurchase flow (وقت استلام بضاعة)، مش من أي endpoint بيسمح للمستخدم
  /// يعدّل التكلفة يدوياً.
  /// </summary>
  public Result<Success> ApplyPurchaseCost(Money newUnitCost)
  {
    if (newUnitCost.Amount <= 0)
      return ProductErrors.InvalidCost;

    Cost = newUnitCost;
    return Result.Success;
  }

  public Result<Success> ChangeMinimumStock(int minimumStock)
  {
    if (minimumStock < 0)
      return ProductErrors.InvalidMinimumStock;

    MinimumStock = minimumStock;
    return Result.Success;
  }

  public Result<Success> Deactivate()
  {
    if (!IsActive)
      return ProductErrors.AlreadyInactive;

    IsActive = false;
    return Result.Success;
  }

  public Result<Success> Activate()
  {
    if (IsActive)
      return ProductErrors.AlreadyActive;

    IsActive = true;
    return Result.Success;
  }

  public Result<Success> EnsureSellable()
  {
    if (!IsActive)
      return ProductErrors.NotSellable;

    return Result.Success;
  }
}
