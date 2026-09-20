namespace TradeFlow.Application.Purchasing.PurchaseOrders.Queries.GetPurchaseOrders;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Purchasing.PurchaseOrders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Purchasing;

public sealed class GetPurchaseOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetPurchaseOrdersQuery, PaginatedList<PurchaseOrderDto>>
{
  public Task<PaginatedList<PurchaseOrderDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
  {
    var query = context.PurchaseOrders.AsQueryable();

    if (request.SupplierId.HasValue)
      query = query.Where(o => o.SupplierId == new SupplierId(request.SupplierId.Value));

    if (!string.IsNullOrWhiteSpace(request.Status) &&
        Enum.TryParse<PurchaseOrderStatus>(request.Status, true, out var status))
    {
      query = query.Where(o => o.Status == status);
    }

    var projected = query.OrderByDescending(o => o.CreatedAt).ProjectTo<PurchaseOrderDto>(mapper.ConfigurationProvider);
    return PaginatedList<PurchaseOrderDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
  }
}
