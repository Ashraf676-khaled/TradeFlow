namespace TradeFlow.Application.Purchasing.Suppliers.Queries.GetSupplierById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Features.Purchasing.Suppliers.Dtos;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class GetSupplierByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
  public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken ct)
  {
    var dto = await context.Suppliers
        .Where(s => s.Id == new SupplierId(request.SupplierId))
        .ProjectTo<SupplierDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? SupplierErrors.NotFound : dto;
  }
}
