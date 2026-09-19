namespace TradeFlow.Application.Inventory.StockItems.Commands.TransferStock;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;

public sealed class TransferStockCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<TransferStockCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(TransferStockCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "لا يمكن تحديد الشركة الحالية.");

    var productId = new ProductId(request.ProductId);
    var fromWarehouseId = new WarehouseId(request.FromWarehouseId);
    var toWarehouseId = new WarehouseId(request.ToWarehouseId);

    var quantityResult = Quantity.Create(request.Quantity);
    if (quantityResult.IsError)
      return quantityResult.Errors;

    var sourceStock = await context.StockItems
        .FirstOrDefaultAsync(s => s.WarehouseId == fromWarehouseId && s.ProductId == productId, ct);

    if (sourceStock is null)
      return StockItemErrors.NotFound;

    var transferOutResult = sourceStock.TransferOut(quantityResult.Value);
    if (transferOutResult.IsError)
      return transferOutResult.Errors;

    var destinationStock = await context.StockItems
        .FirstOrDefaultAsync(s => s.WarehouseId == toWarehouseId && s.ProductId == productId, ct);

    if (destinationStock is null)
    {
      var createResult = StockItem.Create(new TenantId(currentUser.TenantId.Value), toWarehouseId, productId);
      if (createResult.IsError)
        return createResult.Errors;

      destinationStock = createResult.Value;
      context.StockItems.Add(destinationStock);
    }

    var receiveResult = destinationStock.ReceiveStock(quantityResult.Value);
    if (receiveResult.IsError)
      return receiveResult.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
