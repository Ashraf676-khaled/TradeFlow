namespace TradeFlow.Application.Users.Commands.RevokeToken;

using MediatR;
using TradeFlow.Domain.Common.Results;

public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Result<Success>>;
