namespace TradeFlow.Application.Users.Commands.EnableTwoFactor;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record EnableTwoFactorCommand(string Email) : IRequest<Result<AuthResult>>;
