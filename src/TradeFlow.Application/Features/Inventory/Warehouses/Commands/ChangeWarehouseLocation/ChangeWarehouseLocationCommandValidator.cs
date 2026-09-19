namespace TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;

using FluentValidation;

public sealed class ChangeWarehouseLocationCommandValidator : AbstractValidator<ChangeWarehouseLocationCommand>
{
  public ChangeWarehouseLocationCommandValidator()
  {
    RuleFor(x => x.WarehouseId).NotEmpty();
    RuleFor(x => x.NewLocation).NotEmpty().MaximumLength(300);
  }
}
