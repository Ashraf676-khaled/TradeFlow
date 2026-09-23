namespace TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateInvoiceFromOrderCommand(Guid SalesOrderId, int DueInDays = 30) : IRequest<Result<Guid>>;
