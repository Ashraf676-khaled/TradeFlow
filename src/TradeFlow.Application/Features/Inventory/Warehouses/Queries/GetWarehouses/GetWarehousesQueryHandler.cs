namespace TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouses;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Inventory.Warehouses;

public sealed class GetWarehousesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetWarehousesQuery, PaginatedList<WarehouseDto>>
{
  public Task<PaginatedList<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
  {
    var query = context.Warehouses.AsQueryable();

    if (request.IsActive.HasValue)
      query = query.Where(w => w.IsActive == request.IsActive.Value);

    var projected = query.OrderBy(w => w.Name).ProjectTo<WarehouseDto>(mapper.ConfigurationProvider);
    return PaginatedList<WarehouseDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
  }
}
