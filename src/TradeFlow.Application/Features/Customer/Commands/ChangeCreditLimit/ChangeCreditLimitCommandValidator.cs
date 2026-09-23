namespace TradeFlow.Application.Customers.Commands.ChangeCreditLimit;

using FluentValidation;

public sealed class ChangeCreditLimitCommandValidator : AbstractValidator<ChangeCreditLimitCommand>
{
  public ChangeCreditLimitCommandValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty();
    RuleFor(x => x.NewLimit).GreaterThanOrEqualTo(0);
  }
}
