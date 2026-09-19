namespace TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ChangeWarehouseLocationCommand(Guid WarehouseId, string NewLocation) : IRequest<Result<Success>>;
