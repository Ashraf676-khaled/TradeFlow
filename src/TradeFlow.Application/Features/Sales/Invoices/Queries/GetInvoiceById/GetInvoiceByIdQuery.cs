namespace TradeFlow.Application.Sales.Invoices.Queries.GetInvoiceById;

using MediatR;
using TradeFlow.Application.Sales.Invoices;
using TradeFlow.Domain.Common.Results;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
