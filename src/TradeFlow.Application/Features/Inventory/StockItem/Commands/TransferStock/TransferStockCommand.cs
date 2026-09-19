namespace TradeFlow.Application.Inventory.StockItems.Commands.TransferStock;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record TransferStockCommand(
    Guid ProductId, Guid FromWarehouseId, Guid ToWarehouseId, int Quantity) : IRequest<Result<Success>>;
