namespace TradeFlow.Application.Users.Commands.VerifyOtp;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class VerifyOtpCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyOtpCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(
      VerifyOtpCommand request,
      CancellationToken cancellationToken)
      => identityService.VerifyOtpAsync(request.Email, request.Code, cancellationToken);
}
