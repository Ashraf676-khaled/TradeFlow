// Commands/RefreshToken/RefreshTokenCommandValidator.cs
namespace TradeFlow.Application.Users.Commands.RefreshToken;

using FluentValidation;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
  public RefreshTokenCommandValidator()
  {
    RuleFor(x => x.AccessToken).NotEmpty();
    RuleFor(x => x.RefreshToken).NotEmpty();
  }
}
