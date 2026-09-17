namespace TradeFlow.Application.Users.Commands.RevokeToken;

using FluentValidation;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
  public RevokeTokenCommandValidator()
  {
    RuleFor(x => x.RefreshToken).NotEmpty();
  }
}
