namespace TradeFlow.Application.Purchasing.PurchaseOrders.Commands.SubmitPurchaseOrder;

using FluentValidation;

public sealed class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
  public SubmitPurchaseOrderCommandValidator()
  {
    RuleFor(x => x.PurchaseOrderId).NotEmpty();
  }
}
