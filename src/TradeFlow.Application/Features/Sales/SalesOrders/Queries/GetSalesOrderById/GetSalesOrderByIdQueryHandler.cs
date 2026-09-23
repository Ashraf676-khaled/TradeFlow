namespace TradeFlow.Application.Sales.SalesOrders.Queries.GetSalesOrderById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Sales.SalesOrders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Sales;

public sealed class GetSalesOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSalesOrderByIdQuery, Result<SalesOrderDto>>
{
  public async Task<Result<SalesOrderDto>> Handle(GetSalesOrderByIdQuery request, CancellationToken ct)
  {
    var dto = await context.SalesOrders
        .Where(o => o.Id == new SalesOrderId(request.SalesOrderId))
        .ProjectTo<SalesOrderDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? SalesOrderErrors.NotFound : dto;
  }
}
