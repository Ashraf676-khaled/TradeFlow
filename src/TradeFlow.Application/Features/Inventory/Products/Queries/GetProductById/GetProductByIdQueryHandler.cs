// Queries/GetProductById/GetProductByIdQueryHandler.cs
namespace TradeFlow.Application.Inventory.Products.Queries.GetProductById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Inventory;

public sealed class GetProductByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
  public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
  {
    var dto = await context.Products
        .Where(p => p.Id == new ProductId(request.ProductId))
        .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? ProductErrors.NotFound : dto;
  }
}
