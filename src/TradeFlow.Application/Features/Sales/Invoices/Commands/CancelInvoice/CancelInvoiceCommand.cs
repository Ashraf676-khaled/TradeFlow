namespace TradeFlow.Application.Sales.Invoices.Commands.CancelInvoice;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CancelInvoiceCommand(Guid InvoiceId) : IRequest<Result<Success>>;
