namespace TradeFlow.Application.Sales.Invoices.Queries.GetInvoices;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Sales.Invoices;
using TradeFlow.Domain.Common.Identifiers;

public sealed class GetInvoicesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetInvoicesQuery, PaginatedList<InvoiceDto>>
{
  public async Task<PaginatedList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken ct)
  {
    var query = context.Invoices.AsQueryable();

    if (request.CustomerId.HasValue)
      query = query.Where(i => i.CustomerId == new CustomerId(request.CustomerId.Value));

    var projected = query.OrderByDescending(i => i.IssuedAt).ProjectTo<InvoiceDto>(mapper.ConfigurationProvider);
    return await PaginatedList<InvoiceDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
  }
}
