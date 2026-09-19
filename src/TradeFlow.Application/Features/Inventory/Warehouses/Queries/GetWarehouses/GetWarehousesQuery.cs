namespace TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouses;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Inventory.Warehouses;

public sealed record GetWarehousesQuery(int PageNumber = 1, int PageSize = 20, bool? IsActive = null)
    : IRequest<PaginatedList<WarehouseDto>>;
