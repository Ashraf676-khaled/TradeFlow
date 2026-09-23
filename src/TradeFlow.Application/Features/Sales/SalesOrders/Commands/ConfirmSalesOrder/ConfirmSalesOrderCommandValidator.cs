namespace TradeFlow.Application.Sales.SalesOrders.Commands.ConfirmSalesOrder;

using FluentValidation;

public sealed class ConfirmSalesOrderCommandValidator : AbstractValidator<ConfirmSalesOrderCommand>
{
  public ConfirmSalesOrderCommandValidator()
  {
    RuleFor(x => x.SalesOrderId).NotEmpty();
  }
}
