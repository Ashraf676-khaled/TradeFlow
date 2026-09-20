namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;

using FluentValidation;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
  public CreatePurchaseOrderCommandValidator()
  {
    RuleFor(x => x.SupplierId).NotEmpty();
    RuleFor(x => x.WarehouseId).NotEmpty();
    RuleFor(x => x.Items).NotEmpty().WithMessage("Purchase order must contain at least one item.");

    RuleForEach(x => x.Items).ChildRules(item =>
    {
      item.RuleFor(i => i.ProductId).NotEmpty();
      item.RuleFor(i => i.Quantity).GreaterThan(0);
      item.RuleFor(i => i.UnitCost).GreaterThan(0);
    });
  }
}
