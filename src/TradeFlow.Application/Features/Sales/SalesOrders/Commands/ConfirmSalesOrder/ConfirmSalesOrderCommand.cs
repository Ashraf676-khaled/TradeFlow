namespace TradeFlow.Application.Sales.SalesOrders.Commands.ConfirmSalesOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ConfirmSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<ConfirmSalesOrderResult>>;

/// <summary>
/// Returned after a successful confirmation: the order is confirmed, inventory is
/// reserved and the invoice was generated automatically in the same transaction.
/// </summary>
public sealed record ConfirmSalesOrderResult(Guid SalesOrderId, Guid InvoiceId);
