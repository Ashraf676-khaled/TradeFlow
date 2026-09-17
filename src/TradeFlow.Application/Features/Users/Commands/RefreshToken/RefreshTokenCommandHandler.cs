// Commands/RefreshToken/RefreshTokenCommandHandler.cs
namespace TradeFlow.Application.Users.Commands.RefreshToken;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class RefreshTokenCommandHandler(IIdentityService identityService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
      => identityService.RefreshAsync(request.AccessToken, request.RefreshToken, cancellationToken);
}
