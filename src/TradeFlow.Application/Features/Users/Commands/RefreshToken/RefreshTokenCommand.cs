// Commands/RefreshToken/RefreshTokenCommand.cs
namespace TradeFlow.Application.Users.Commands.RefreshToken;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : IRequest<Result<AuthResult>>;
