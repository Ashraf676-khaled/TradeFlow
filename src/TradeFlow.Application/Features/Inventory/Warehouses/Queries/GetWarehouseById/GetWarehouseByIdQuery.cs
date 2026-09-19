namespace TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouseById;

using MediatR;
using TradeFlow.Application.Inventory.Warehouses;
using TradeFlow.Domain.Common.Results;

public sealed record GetWarehouseByIdQuery(Guid WarehouseId) : IRequest<Result<WarehouseDto>>;
