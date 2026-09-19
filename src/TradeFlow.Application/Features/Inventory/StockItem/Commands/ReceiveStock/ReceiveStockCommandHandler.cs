namespace TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;

public sealed class ReceiveStockCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<ReceiveStockCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(ReceiveStockCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "The current company cannot be determined..");

    var productId = new ProductId(request.ProductId);
    var warehouseId = new WarehouseId(request.WarehouseId);

    var productExists = await context.Products.AnyAsync(p => p.Id == productId, ct);
    if (!productExists)
      return ProductErrors.NotFound;

    var warehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.Id == warehouseId, ct);
    if (warehouse is null)
      return WarehouseErrors.NotFound;

    var ensureOperationalResult = warehouse.EnsureOperational();
    if (ensureOperationalResult.IsError)
      return ensureOperationalResult.Errors;

    var quantityResult = Quantity.Create(request.Quantity);
    if (quantityResult.IsError)
      return quantityResult.Errors;

    var stockItem = await context.StockItems
        .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);

    if (stockItem is null)
    {
      var createResult = StockItem.Create(new TenantId(currentUser.TenantId.Value), warehouseId, productId);
      if (createResult.IsError)
        return createResult.Errors;

      stockItem = createResult.Value;
      context.StockItems.Add(stockItem);
    }

    var receiveResult = stockItem.ReceiveStock(quantityResult.Value);
    if (receiveResult.IsError)
      return receiveResult.Errors;

    await context.SaveChangesAsync(ct);
    return stockItem.Id.Value;
  }
}
