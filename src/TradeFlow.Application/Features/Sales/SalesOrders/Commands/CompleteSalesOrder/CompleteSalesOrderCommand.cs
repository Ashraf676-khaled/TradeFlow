namespace TradeFlow.Application.Sales.SalesOrders.Commands.CompleteSalesOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CompleteSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<Success>>;
