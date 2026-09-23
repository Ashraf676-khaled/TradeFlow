namespace TradeFlow.Application.Sales.Invoices.Commands.RegisterPayment;

using FluentValidation;

public sealed class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
  public RegisterPaymentCommandValidator()
  {
    RuleFor(x => x.InvoiceId).NotEmpty();
    RuleFor(x => x.Amount).GreaterThan(0);
  }
}
