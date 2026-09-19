namespace TradeFlow.Application.Inventory.StockItems.Queries.GetStockItem;

using MediatR;
using TradeFlow.Application.Inventory.StockItems;
using TradeFlow.Domain.Common.Results;

public sealed record GetStockItemQuery(Guid ProductId, Guid WarehouseId) : IRequest<Result<StockItemDto>>;
