namespace TradeFlow.Application.Inventory.Warehouses.Commands.ActivateWarehouse;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ActivateWarehouseCommand(Guid WarehouseId) : IRequest<Result<Success>>;
