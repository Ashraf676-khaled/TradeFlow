namespace TradeFlow.Application.Inventory.StockItems.Queries.GetStockItem;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Inventory.StockItems;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class GetStockItemQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetStockItemQuery, Result<StockItemDto>>
{
  public async Task<Result<StockItemDto>> Handle(GetStockItemQuery request, CancellationToken ct)
  {
    var dto = await context.StockItems
        .Where(s => s.ProductId == new ProductId(request.ProductId)
                 && s.WarehouseId == new WarehouseId(request.WarehouseId))
        .ProjectTo<StockItemDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? StockItemErrors.NotFound : dto;
  }
}
