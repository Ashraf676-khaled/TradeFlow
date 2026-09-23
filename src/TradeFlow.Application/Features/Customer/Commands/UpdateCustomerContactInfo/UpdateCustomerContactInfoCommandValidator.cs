namespace TradeFlow.Application.Customers.Commands.UpdateCustomerContactInfo;

using FluentValidation;

public sealed class UpdateCustomerContactInfoCommandValidator : AbstractValidator<UpdateCustomerContactInfoCommand>
{
  public UpdateCustomerContactInfoCommandValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty();
    RuleFor(x => x.Phone).NotEmpty();
    RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
  }
}
