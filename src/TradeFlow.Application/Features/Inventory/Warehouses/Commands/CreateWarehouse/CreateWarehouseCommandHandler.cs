namespace TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class CreateWarehouseCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant", "The current company cannot be determined.");

    var nameExists = await context.Warehouses
        .AnyAsync(w => w.Name == request.Name, ct);

    if (nameExists)
      return WarehouseErrors.NameAlreadyExists;

    var result = Warehouse.Create(new TenantId(currentUser.TenantId.Value), request.Name, request.Location);
    if (result.IsError)
      return result.Errors;

    context.Warehouses.Add(result.Value);
    await context.SaveChangesAsync(ct);

    return result.Value.Id.Value;
  }
}
