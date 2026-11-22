namespace TradeFlow.Application.Users.Commands.GoogleLogin;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record GoogleLoginCommand(string IdToken) : IRequest<Result<AuthResult>>;
