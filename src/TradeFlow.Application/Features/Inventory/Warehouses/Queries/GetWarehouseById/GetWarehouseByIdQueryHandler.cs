namespace TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouseById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Inventory.Warehouses;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class GetWarehouseByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
  public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken ct)
  {
    var dto = await context.Warehouses
        .Where(w => w.Id == new WarehouseId(request.WarehouseId))
        .ProjectTo<WarehouseDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? WarehouseErrors.NotFound : dto;
  }
}
