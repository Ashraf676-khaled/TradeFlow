namespace TradeFlow.Application.Customers.Commands.CreateCustomer;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record CreateCustomerCommand(
    string Name, string Phone, decimal CreditLimit, string? Email = null) : IRequest<Result<Guid>>;
