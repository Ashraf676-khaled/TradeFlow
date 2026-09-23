namespace TradeFlow.Application.Customers.Commands.UpdateCustomerContactInfo;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record UpdateCustomerContactInfoCommand(
    Guid CustomerId, string Phone, string? Email = null) : IRequest<Result<Success>>;
