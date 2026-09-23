namespace TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrderById;

using MediatR;
using TradeFlow.Application.Sales.SalesOrders;
using TradeFlow.Domain.Common.Results;

public sealed record GetSalesOrderByIdQuery(Guid SalesOrderId) : IRequest<Result<SalesOrderDto>>;
