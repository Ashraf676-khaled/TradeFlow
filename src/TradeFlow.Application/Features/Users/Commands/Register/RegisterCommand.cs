namespace TradeFlow.Application.Users.Commands.Register;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record RegisterCommand(
    string CompanyName,
    string FullName,
    string Email,
    string Password) : IRequest<Result<AuthResult>>;
