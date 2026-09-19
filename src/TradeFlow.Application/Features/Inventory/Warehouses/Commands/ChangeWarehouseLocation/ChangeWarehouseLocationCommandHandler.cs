namespace TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class ChangeWarehouseLocationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeWarehouseLocationCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(ChangeWarehouseLocationCommand request, CancellationToken ct)
  {
    var warehouse = await context.Warehouses
        .FirstOrDefaultAsync(w => w.Id == new WarehouseId(request.WarehouseId), ct);

    if (warehouse is null)
      return WarehouseErrors.NotFound;

    var result = warehouse.ChangeLocation(request.NewLocation);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
