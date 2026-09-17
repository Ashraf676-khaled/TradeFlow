// Commands/Login/LoginCommandValidator.cs
namespace TradeFlow.Application.Users.Commands.Login;

using FluentValidation;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
  public LoginCommandValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.Password).NotEmpty();
  }
}
