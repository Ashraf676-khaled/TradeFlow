namespace TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrders;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Sales.SalesOrders;

public sealed record GetSalesOrdersQuery(
    int PageNumber = 1, int PageSize = 20, string? Status = null, Guid? CustomerId = null)
    : IRequest<PaginatedList<SalesOrderDto>>;
