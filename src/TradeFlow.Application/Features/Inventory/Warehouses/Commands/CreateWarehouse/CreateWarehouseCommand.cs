namespace TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateWarehouseCommand(string Name, string Location) : IRequest<Result<Guid>>;
