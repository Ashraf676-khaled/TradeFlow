namespace TradeFlow.Application.Inventory.StockItems.Queries.GetStockByWarehouse;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Inventory.StockItems;

public sealed record GetStockByWarehouseQuery(Guid WarehouseId, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<StockItemDto>>;
