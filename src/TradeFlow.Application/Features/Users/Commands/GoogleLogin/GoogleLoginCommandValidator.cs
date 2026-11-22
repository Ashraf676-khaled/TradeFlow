namespace TradeFlow.Application.Users.Commands.GoogleLogin;

using FluentValidation;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
  public GoogleLoginCommandValidator()
  {
    RuleFor(command => command.IdToken).NotEmpty().MaximumLength(8192);
  }
}
