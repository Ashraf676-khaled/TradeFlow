namespace TradeFlow.Application.Purchasing.Suppliers.Queries.GetSuppliers;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Features.Purchasing.Suppliers.Dtos;
using TradeFlow.Application.Purchasing.Suppliers;

public sealed class GetSuppliersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSuppliersQuery, PaginatedList<SupplierDto>>
{
  public Task<PaginatedList<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken ct)
  {
    var query = context.Suppliers.AsQueryable();

    if (request.IsActive.HasValue)
      query = query.Where(s => s.IsActive == request.IsActive.Value);

    var projected = query.OrderBy(s => s.Name).ProjectTo<SupplierDto>(mapper.ConfigurationProvider);
    return PaginatedList<SupplierDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
  }
}
