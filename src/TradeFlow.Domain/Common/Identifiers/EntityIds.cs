namespace TradeFlow.Domain.Common.Identifiers;

using TradeFlow.Domain.Common.Results;

public readonly record struct ProductId(Guid Value)
{
  public static ProductId New() => new(Guid.CreateVersion7());

  public static Result<ProductId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(ProductId)) : new ProductId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct WarehouseId(Guid Value)
{
  public static WarehouseId New() => new(Guid.CreateVersion7());

  public static Result<WarehouseId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(WarehouseId)) : new WarehouseId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct StockItemId(Guid Value)
{
  public static StockItemId New() => new(Guid.CreateVersion7());

  public static Result<StockItemId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(StockItemId)) : new StockItemId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct CustomerId(Guid Value)
{
  public static CustomerId New() => new(Guid.CreateVersion7());

  public static Result<CustomerId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(CustomerId)) : new CustomerId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct SupplierId(Guid Value)
{
  public static SupplierId New() => new(Guid.CreateVersion7());

  public static Result<SupplierId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(SupplierId)) : new SupplierId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct SalesOrderId(Guid Value)
{
  public static SalesOrderId New() => new(Guid.CreateVersion7());

  public static Result<SalesOrderId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(SalesOrderId)) : new SalesOrderId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct InvoiceId(Guid Value)
{
  public static InvoiceId New() => new(Guid.CreateVersion7());

  public static Result<InvoiceId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(InvoiceId)) : new InvoiceId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct PurchaseOrderId(Guid Value)
{
  public static PurchaseOrderId New() => new(Guid.CreateVersion7());

  public static Result<PurchaseOrderId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(PurchaseOrderId)) : new PurchaseOrderId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct UserId(Guid Value)
{
  public static UserId New() => new(Guid.CreateVersion7());

  public static Result<UserId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(UserId)) : new UserId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct SettingId(Guid Value)
{
  public static SettingId New() => new(Guid.CreateVersion7());

  public static Result<SettingId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(SettingId)) : new SettingId(value);

  public override string ToString() => Value.ToString();
}

public readonly record struct TenantId(Guid Value)
{
  public static TenantId New() => new(Guid.CreateVersion7());

  public static Result<TenantId> Create(Guid value)
      => value == Guid.Empty ? IdentifierErrors.Empty(nameof(TenantId)) : new TenantId(value);

  public override string ToString() => Value.ToString();
}

internal static class IdentifierErrors
{
  public static Error Empty(string idName) => Error.Validation(
      $"{idName}.Empty", $"{idName} cannot be empty.");
}
