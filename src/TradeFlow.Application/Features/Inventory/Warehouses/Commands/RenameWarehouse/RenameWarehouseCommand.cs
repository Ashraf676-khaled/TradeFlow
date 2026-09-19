namespace TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record RenameWarehouseCommand(Guid WarehouseId, string NewName) : IRequest<Result<Success>>;
