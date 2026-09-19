namespace TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class RenameWarehouseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RenameWarehouseCommand, Result<Success>>
{
  public async Task<Result<Success>> Handle(RenameWarehouseCommand request, CancellationToken ct)
  {
    var warehouse = await context.Warehouses
        .FirstOrDefaultAsync(w => w.Id == new WarehouseId(request.WarehouseId), ct);

    if (warehouse is null)
      return WarehouseErrors.NotFound;

    var nameExists = await context.Warehouses
        .AnyAsync(w => w.Name == request.NewName && w.Id != warehouse.Id, ct);

    if (nameExists)
      return WarehouseErrors.NameAlreadyExists;

    var result = warehouse.Rename(request.NewName);
    if (result.IsError)
      return result.Errors;

    await context.SaveChangesAsync(ct);
    return Result.Success;
  }
}
