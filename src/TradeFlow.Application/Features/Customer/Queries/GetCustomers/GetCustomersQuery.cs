namespace TradeFlow.Application.Customers.Queries.GetCustomers;

using MediatR;
using TradeFlow.Application.Common.Models;

public sealed record GetCustomersQuery(int PageNumber = 1, int PageSize = 20, bool? IsActive = null)
    : IRequest<PaginatedList<CustomerDto>>;
