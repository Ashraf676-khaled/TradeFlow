// Commands/Login/LoginCommandHandler.cs
namespace TradeFlow.Application.Users.Commands.Login;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
      => identityService.LoginAsync(request.Email, request.Password, cancellationToken);
}
