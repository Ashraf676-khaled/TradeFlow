namespace TradeFlow.Application.Inventory.StockItems.Commands.TransferStock;

using FluentValidation;

public sealed class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
  public TransferStockCommandValidator()
  {
    RuleFor(x => x.ProductId).NotEmpty();
    RuleFor(x => x.FromWarehouseId).NotEmpty();
    RuleFor(x => x.ToWarehouseId).NotEmpty();
    RuleFor(x => x.Quantity).GreaterThan(0);
    RuleFor(x => x).Must(x => x.FromWarehouseId != x.ToWarehouseId)
        .WithMessage("Transfer to the same warehouse is not possible.");
  }
}
