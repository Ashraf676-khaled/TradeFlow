namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ApprovePurchaseOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ApprovePurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<Result<Success>>;
