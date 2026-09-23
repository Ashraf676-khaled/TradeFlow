namespace TradeFlow.Application.Customers.Queries.GetCustomers;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Common.Models;

public sealed class GetCustomersQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
{
  public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken ct)
  {
    var query = context.Customers.AsQueryable();

    if (request.IsActive.HasValue)
      query = query.Where(c => c.IsActive == request.IsActive.Value);

    var totalCount = await query.CountAsync(ct);
    var customers = await query
        .OrderBy(c => c.Name)
        .Skip((request.PageNumber - 1) * request.PageSize)
        .Take(request.PageSize)
        .ToListAsync(ct);

    var items = customers.Select(mapper.Map<CustomerDto>).ToList();
    return new PaginatedList<CustomerDto>(items, totalCount, request.PageNumber, request.PageSize);
  }
}
