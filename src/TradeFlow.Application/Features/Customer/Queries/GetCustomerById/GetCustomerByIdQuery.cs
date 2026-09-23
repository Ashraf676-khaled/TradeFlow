namespace TradeFlow.Application.Customers.Queries.GetCustomerById;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;
