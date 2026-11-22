namespace TradeFlow.Application.Users.Commands.EnableTwoFactor;

using FluentValidation;

public sealed class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
  public EnableTwoFactorCommandValidator()
  {
    RuleFor(command => command.Email).NotEmpty().EmailAddress();
  }
}
