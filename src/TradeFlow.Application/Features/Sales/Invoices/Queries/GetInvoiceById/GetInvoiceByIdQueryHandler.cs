namespace TradeFlow.Application.Sales.Invoices.Queries.GetInvoiceById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Sales.Invoices;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Sales;

public sealed class GetInvoiceByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
  public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
  {
    var dto = await context.Invoices
        .Where(i => i.Id == new InvoiceId(request.InvoiceId))
        .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? InvoiceErrors.NotFound : dto;
  }
}
