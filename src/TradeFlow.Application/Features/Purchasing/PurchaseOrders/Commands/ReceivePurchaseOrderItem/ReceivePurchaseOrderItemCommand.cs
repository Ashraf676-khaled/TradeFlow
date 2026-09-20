namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ReceivePurchaseOrderItem;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ReceivePurchaseOrderItemCommand(
    Guid PurchaseOrderId, Guid ProductId, int ReceivedQuantity) : IRequest<Result<Success>>;
