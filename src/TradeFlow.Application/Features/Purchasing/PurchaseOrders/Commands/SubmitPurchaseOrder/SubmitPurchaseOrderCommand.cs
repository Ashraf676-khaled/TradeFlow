namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.SubmitPurchaseOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record SubmitPurchaseOrderCommand(Guid PurchaseOrderId) : IRequest<Result<Success>>;
