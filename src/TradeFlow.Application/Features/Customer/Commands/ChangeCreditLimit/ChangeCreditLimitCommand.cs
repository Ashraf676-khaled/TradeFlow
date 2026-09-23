namespace TradeFlow.Application.Customers.Commands.ChangeCreditLimit;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record ChangeCreditLimitCommand(Guid CustomerId, decimal NewLimit) : IRequest<Result<Success>>;
