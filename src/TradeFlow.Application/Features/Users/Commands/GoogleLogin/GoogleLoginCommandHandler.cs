namespace TradeFlow.Application.Users.Commands.GoogleLogin;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class GoogleLoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<GoogleLoginCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(
      GoogleLoginCommand request,
      CancellationToken cancellationToken)
      => identityService.GoogleLoginAsync(request.IdToken, cancellationToken);
}
