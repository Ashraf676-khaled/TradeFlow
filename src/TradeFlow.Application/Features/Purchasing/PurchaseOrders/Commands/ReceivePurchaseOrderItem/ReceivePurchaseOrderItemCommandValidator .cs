namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.ReceivePurchaseOrderItem;

using FluentValidation;

public sealed class ReceivePurchaseOrderItemCommandValidator : AbstractValidator<ReceivePurchaseOrderItemCommand>
{
  public ReceivePurchaseOrderItemCommandValidator()
  {
    RuleFor(x => x.PurchaseOrderId).NotEmpty();
    RuleFor(x => x.ProductId).NotEmpty();
    RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
  }
}
