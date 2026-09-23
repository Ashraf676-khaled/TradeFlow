namespace TradeFlow.Application.Customers.Commands.ActivateCustomer;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ActivateCustomerCommand(Guid CustomerId) : IRequest<Result<Success>>;
