namespace TradeFlow.Application.Inventory.Products.Queries.GetProducts;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;

public sealed class GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
  public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
  {
    var query = context.Products.AsQueryable();

    if (request.IsActive.HasValue)
      query = query.Where(p => p.IsActive == request.IsActive.Value);

    var projected = query
        .OrderBy(p => p.Name)
        .ProjectTo<ProductDto>(mapper.ConfigurationProvider);

    return await PaginatedList<ProductDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
  }
}
