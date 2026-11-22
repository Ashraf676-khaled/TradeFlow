namespace TradeFlow.Application.Users.Commands.VerifyOtp;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed record VerifyOtpCommand(string Email, string Code) : IRequest<Result<AuthResult>>;
