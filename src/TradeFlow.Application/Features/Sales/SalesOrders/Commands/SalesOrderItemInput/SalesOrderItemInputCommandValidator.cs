namespace TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;

using FluentValidation;

public sealed class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
  public CreateSalesOrderCommandValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty();
    RuleFor(x => x.WarehouseId).NotEmpty();
    RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");

    RuleForEach(x => x.Items).ChildRules(item =>
    {
      item.RuleFor(i => i.ProductId).NotEmpty();
      item.RuleFor(i => i.Quantity).GreaterThan(0);
      item.RuleFor(i => i.UnitPrice).GreaterThan(0);
    });
  }
}
