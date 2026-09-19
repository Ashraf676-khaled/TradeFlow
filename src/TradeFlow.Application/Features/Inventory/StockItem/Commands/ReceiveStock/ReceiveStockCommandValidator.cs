namespace TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;

using FluentValidation;

public sealed class ReceiveStockCommandValidator : AbstractValidator<ReceiveStockCommand>
{
  public ReceiveStockCommandValidator()
  {
    RuleFor(x => x.ProductId).NotEmpty();
    RuleFor(x => x.WarehouseId).NotEmpty();
    RuleFor(x => x.Quantity).GreaterThan(0);
  }
}
