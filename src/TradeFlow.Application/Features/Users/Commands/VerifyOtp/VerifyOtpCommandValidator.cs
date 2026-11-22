namespace TradeFlow.Application.Users.Commands.VerifyOtp;

using FluentValidation;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
  public VerifyOtpCommandValidator()
  {
    RuleFor(command => command.Email).NotEmpty().EmailAddress();
    RuleFor(command => command.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$");
  }
}
