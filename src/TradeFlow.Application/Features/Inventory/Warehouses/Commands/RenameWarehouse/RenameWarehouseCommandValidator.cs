namespace TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;

using FluentValidation;

public sealed class RenameWarehouseCommandValidator : AbstractValidator<RenameWarehouseCommand>
{
  public RenameWarehouseCommandValidator()
  {
    RuleFor(x => x.WarehouseId).NotEmpty();
    RuleFor(x => x.NewName).NotEmpty().MaximumLength(150);
  }
}
