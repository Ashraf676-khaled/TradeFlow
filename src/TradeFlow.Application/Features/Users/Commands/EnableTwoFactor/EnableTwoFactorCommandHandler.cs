namespace TradeFlow.Application.Users.Commands.EnableTwoFactor;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class EnableTwoFactorCommandHandler(IIdentityService identityService)
    : IRequestHandler<EnableTwoFactorCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(
      EnableTwoFactorCommand request,
      CancellationToken cancellationToken)
      => identityService.EnableTwoFactorAsync(request.Email, cancellationToken);
}
