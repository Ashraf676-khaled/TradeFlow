namespace TradeFlow.Application.Customers.Commands.CreateCustomer;

using FluentValidation;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
  public CreateCustomerCommandValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Phone).NotEmpty();
    RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
  }
}
