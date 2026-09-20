namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CancelPurchaseOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CancelPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<Result<Success>>;
