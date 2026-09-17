// Commands/Login/LoginCommand.cs
namespace TradeFlow.Application.Users.Commands.Login;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResult>>;
