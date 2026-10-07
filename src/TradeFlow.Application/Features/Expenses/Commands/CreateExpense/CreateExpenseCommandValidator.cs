namespace TradeFlow.Application.Expenses.Commands.CreateExpense;

using FluentValidation;

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
  public CreateExpenseCommandValidator()
  {
    RuleFor(command => command.Description).NotEmpty().MaximumLength(200);
    RuleFor(command => command.Category).NotEmpty().MaximumLength(60);
    RuleFor(command => command.Classification).Must(value => value is "ثابت" or "متغير");
    RuleFor(command => command.Amount).GreaterThan(0);
    RuleFor(command => command.IncurredAt).NotEqual(default(DateTimeOffset));
  }
}
