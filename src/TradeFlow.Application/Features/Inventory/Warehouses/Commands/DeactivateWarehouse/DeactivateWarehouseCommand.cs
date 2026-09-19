namespace TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record DeactivateWarehouseCommand(Guid WarehouseId) : IRequest<Result<Success>>;
