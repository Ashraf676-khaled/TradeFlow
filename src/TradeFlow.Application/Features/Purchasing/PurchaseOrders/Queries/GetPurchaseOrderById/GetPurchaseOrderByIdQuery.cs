namespace TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrderById;

using MediatR;
using TradeFlow.Application.Purchasing.PurchaseOrders;
using TradeFlow.Domain.Common.Results;

public sealed record GetPurchaseOrderByIdQuery(Guid PurchaseOrderId) : IRequest<Result<PurchaseOrderDto>>;
