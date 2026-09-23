namespace TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouses;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Inventory.Warehouses;
using TradeFlow.Domain.Common.Identifiers;

public sealed class GetWarehousesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<GetWarehousesQuery, PaginatedList<WarehouseDto>>
{
  public async Task<PaginatedList<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
  {
    // Seeding lazily guarantees the warehouse dropdown is never empty for a new tenant.
    if (currentUser.TenantId is not null)
      await DefaultWarehouse.EnsureAsync(context, new TenantId(currentUser.TenantId.Value), ct);

    var query = context.Warehouses.AsQueryable();

    if (request.IsActive.HasValue)
      query = query.Where(w => w.IsActive == request.IsActive.Value);

    var projected = query.OrderBy(w => w.Name).ProjectTo<WarehouseDto>(mapper.ConfigurationProvider);
    return await PaginatedList<WarehouseDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
  }
}
