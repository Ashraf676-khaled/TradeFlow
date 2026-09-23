namespace TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrders;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;
using TradeFlow.Application.Sales.SalesOrders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Sales;

public sealed class GetSalesOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSalesOrdersQuery, PaginatedList<SalesOrderDto>>
{
  public async Task<PaginatedList<SalesOrderDto>> Handle(GetSalesOrdersQuery request, CancellationToken ct)
  {
    var query = context.SalesOrders.AsQueryable();

    if (request.CustomerId.HasValue)
      query = query.Where(o => o.CustomerId == new CustomerId(request.CustomerId.Value));

    if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var status))
      query = query.Where(o => o.Status == status);

    var projected = query.OrderByDescending(o => o.OrderDate).ProjectTo<SalesOrderDto>(mapper.ConfigurationProvider);
    return await PaginatedList<SalesOrderDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
  }
}
