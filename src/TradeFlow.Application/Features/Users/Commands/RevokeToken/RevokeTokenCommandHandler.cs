// Commands/RevokeToken/RevokeTokenCommandHandler.cs
namespace TradeFlow.Application.Users.Commands.RevokeToken;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class RevokeTokenCommandHandler(IIdentityService identityService)
    : IRequestHandler<RevokeTokenCommand, Result<Success>>
{
  public Task<Result<Success>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
      => identityService.RevokeAsync(request.RefreshToken, cancellationToken);
}
