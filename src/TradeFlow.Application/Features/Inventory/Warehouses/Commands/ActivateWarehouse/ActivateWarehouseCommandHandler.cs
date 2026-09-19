namespace TradeFlow.Application.Inventory.Warehouses.Commands.ActivateWarehouse;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class ActivateWarehouseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateWarehouseCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ActivateWarehouseCommand request, CancellationToken ct)
  {
    var warehouse = await context.Warehouses
        .FirstOrDefaultAsync(w => w.Id == new WarehouseId(request.WarehouseId), ct);

    if (warehouse is null)
      return WarehouseErrors.NotFound;

    var result = warehouse.Activate();
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
