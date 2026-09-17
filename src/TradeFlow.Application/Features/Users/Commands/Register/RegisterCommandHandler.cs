// Commands/Register/RegisterCommandHandler.cs
namespace TradeFlow.Application.Users.Commands.Register;

using MediatR;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Results;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<AuthResult>>
{
  public Task<Result<AuthResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    => identityService.RegisterAsync(
        request.CompanyName, request.FullName, request.Email, request.Password, cancellationToken);
}
