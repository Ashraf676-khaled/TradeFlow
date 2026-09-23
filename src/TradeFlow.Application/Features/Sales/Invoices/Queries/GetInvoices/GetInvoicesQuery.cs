namespace TradeFlow.Application.Sales.Invoices.Queries.GetInvoices;

using MediatR;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Sales.Invoices;

public sealed record GetInvoicesQuery(int PageNumber = 1, int PageSize = 20, Guid? CustomerId = null)
    : IRequest<PaginatedList<InvoiceDto>>;
