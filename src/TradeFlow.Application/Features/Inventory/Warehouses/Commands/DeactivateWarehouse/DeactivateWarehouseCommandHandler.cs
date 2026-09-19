namespace TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class DeactivateWarehouseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateWarehouseCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(DeactivateWarehouseCommand request, CancellationToken ct)
  {
    var warehouse = await context.Warehouses
        .FirstOrDefaultAsync(w => w.Id == new WarehouseId(request.WarehouseId), ct);

    if (warehouse is null)
      return WarehouseErrors.NotFound;

    // Cross-Aggregate Check: هل فيه StockItems فيها كمية في المخزن ده؟
    var hasStock = await context.StockItems
        .AnyAsync(s => s.WarehouseId == warehouse.Id &&
                      (s.AvailableQuantity.Value > 0 || s.ReservedQuantity.Value > 0), ct);

    if (hasStock)
      return WarehouseErrors.HasStockOrActiveOrders;

    var result = warehouse.Deactivate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
