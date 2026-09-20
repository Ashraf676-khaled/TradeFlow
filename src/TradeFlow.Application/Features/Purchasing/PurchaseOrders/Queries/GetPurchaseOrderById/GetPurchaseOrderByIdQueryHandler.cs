namespace TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrderById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Purchasing.PurchaseOrders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Purchasing;

public sealed class GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
  public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken ct)
  {
    var dto = await context.PurchaseOrders
        .Where(o => o.Id == new PurchaseOrderId(request.PurchaseOrderId))
        .ProjectTo<PurchaseOrderDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? PurchaseOrderErrors.NotFound : dto;
  }
}
