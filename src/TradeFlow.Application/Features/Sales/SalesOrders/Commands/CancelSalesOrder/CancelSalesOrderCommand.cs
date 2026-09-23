namespace TradeFlow.Application.Sales.SalesOrders.Commands.CancelSalesOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CancelSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<Success>>;
