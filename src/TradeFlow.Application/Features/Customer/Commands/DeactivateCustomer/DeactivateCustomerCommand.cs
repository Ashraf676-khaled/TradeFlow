namespace TradeFlow.Application.Customers.Commands.DeactivateCustomer;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record DeactivateCustomerCommand(Guid CustomerId) : IRequest<Result<Success>>;
