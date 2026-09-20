namespace TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrders;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Purchasing.PurchaseOrders;

public sealed record GetPurchaseOrdersQuery(
    int PageNumber = 1, int PageSize = 20, string? Status = null, Guid? SupplierId = null)
    : IRequest<PaginatedList<PurchaseOrderDto>>;
