namespace TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;

using FluentValidation;

public sealed class CreateInvoiceFromOrderCommandValidator : AbstractValidator<CreateInvoiceFromOrderCommand>
{
  public CreateInvoiceFromOrderCommandValidator()
  {
    RuleFor(x => x.SalesOrderId).NotEmpty();
    RuleFor(x => x.DueInDays).GreaterThan(0);
  }
}
