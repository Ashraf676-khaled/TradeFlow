namespace TradeFlow.Application.Users.Commands.Register;

using FluentValidation;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
  public RegisterCommandValidator()
  {
    RuleFor(x => x.CompanyName)
        .NotEmpty().WithMessage("Company name is required.")
        .MaximumLength(200);

    RuleFor(x => x.FullName)
        .NotEmpty().WithMessage("Full name is required.")
        .MaximumLength(150);

    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().WithMessage("Invalid email format.");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.")
        .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
        .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
        .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
  }
}
