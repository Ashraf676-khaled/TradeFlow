namespace TradeFlow.Application.Customers.Queries.GetCustomerById;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Customers;

public sealed class GetCustomerByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
  public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
  {
    var dto = await context.Customers
        .Where(c => c.Id == new CustomerId(request.CustomerId))
        .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);

    return dto is null ? CustomerErrors.NotFound : dto;
  }
}
