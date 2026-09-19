namespace TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ReceiveStockCommand(Guid ProductId, Guid WarehouseId, int Quantity) : IRequest<Result<Guid>>;
