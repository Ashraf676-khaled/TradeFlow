namespace TradeFlow.Application.Inventory.Products.Commands.CreateProduct;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Inventory.Warehouses;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;

public sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant","Unable to determine the current tenant.");

    var tenantId = new TenantId(currentUser.TenantId.Value);

    var skuResult = Sku.Create(request.Sku);
    if (skuResult.IsError)
      return skuResult.Errors;

    // فحص التكرار قبل الحفظ — عشان نرجّع 409 واضح بدل ما الداتابيز ترفض الـInsert
    var skuExists = await context.Products
        .AnyAsync(p => p.Sku.Value == skuResult.Value.Value, ct);

    if (skuExists)
      return ProductErrors.SkuAlreadyExists;

    var sellingPriceResult = Money.EGP(request.SellingPrice);
    if (sellingPriceResult.IsError)
      return sellingPriceResult.Errors;

    var costResult = Money.EGP(request.Cost);
    if (costResult.IsError)
      return costResult.Errors;

    // Resolve the opening-stock warehouse BEFORE adding the product so the
    // default-warehouse seeding save cannot flush the pending product early.
    Warehouse? openingStockWarehouse = null;
    if (request.OpeningStockQuantity > 0)
    {
      var warehouseResult = await ResolveOpeningStockWarehouseAsync(request, tenantId, ct);
      if (warehouseResult.IsError)
        return warehouseResult.Errors;

      openingStockWarehouse = warehouseResult.Value;
    }

    var productResult = Product.Create(
        tenantId,
        request.Name,
        skuResult.Value,
        sellingPriceResult.Value,
        costResult.Value,
        request.MinimumStock);

    if (productResult.IsError)
      return productResult.Errors;

    var product = productResult.Value;
    context.Products.Add(product);

    if (openingStockWarehouse is not null)
    {
      var stockResult = await AddOpeningStockAsync(
          product, openingStockWarehouse, tenantId, request.OpeningStockQuantity, ct);
      if (stockResult.IsError)
        return stockResult.Errors;
    }

    // Single SaveChanges → product and its opening stock are persisted atomically.
    await context.SaveChangesAsync(ct);

    return product.Id.Value;
  }

  private async Task<Result<Warehouse>> ResolveOpeningStockWarehouseAsync(
      CreateProductCommand request, TenantId tenantId, CancellationToken ct)
  {
    if (request.OpeningStockWarehouseId is { } warehouseIdValue)
    {
      var warehouse = await context.Warehouses
          .FirstOrDefaultAsync(w => w.Id == new WarehouseId(warehouseIdValue), ct);

      if (warehouse is null)
        return WarehouseErrors.NotFound;

      var operationalResult = warehouse.EnsureOperational();
      if (operationalResult.IsError)
        return operationalResult.Errors;

      return warehouse;
    }

    // No warehouse chosen → fall back to the (lazily auto-created) default warehouse.
    return await DefaultWarehouse.EnsureAsync(context, tenantId, ct);
  }

  private async Task<Result<Success>> AddOpeningStockAsync(
      Product product,
      Warehouse warehouse,
      TenantId tenantId,
      int quantity,
      CancellationToken ct)
  {
    var quantityResult = Quantity.Create(quantity);
    if (quantityResult.IsError)
      return quantityResult.Errors;

    var warehouseId = warehouse.Id;
    var stockItem = await context.StockItems
        .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == product.Id, ct);

    if (stockItem is null)
    {
      var createResult = StockItem.Create(tenantId, warehouseId, product.Id);
      if (createResult.IsError)
        return createResult.Errors;

      stockItem = createResult.Value;
      context.StockItems.Add(stockItem);
    }

    var receiveResult = stockItem.ReceiveStock(quantityResult.Value);
    if (receiveResult.IsError)
      return receiveResult.Errors;

    return Result.Success;
  }
}
