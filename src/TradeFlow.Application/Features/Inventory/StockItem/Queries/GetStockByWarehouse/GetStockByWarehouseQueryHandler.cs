namespace TradeFlow.Application.Inventory.StockItems.Queries.GetStockByWarehouse;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Inventory.StockItems;
using TradeFlow.Domain.Common.Identifiers;

public sealed class GetStockByWarehouseQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetStockByWarehouseQuery, PaginatedList<StockItemDto>>
{
  public Task<PaginatedList<StockItemDto>> Handle(GetStockByWarehouseQuery request, CancellationToken ct)
  {
    var projected = context.StockItems
        .Where(s => s.WarehouseId == new WarehouseId(request.WarehouseId))
        .ProjectTo<StockItemDto>(mapper.ConfigurationProvider);

    return PaginatedList<StockItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
  }
}
