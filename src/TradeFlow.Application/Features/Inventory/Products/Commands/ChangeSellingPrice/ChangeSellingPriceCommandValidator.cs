// Commands/ChangeSellingPrice/ChangeSellingPriceCommandValidator.cs
namespace TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;

using FluentValidation;

public sealed class ChangeSellingPriceCommandValidator : AbstractValidator<ChangeSellingPriceCommand>
{
  public ChangeSellingPriceCommandValidator()
  {
    RuleFor(x => x.ProductId).NotEmpty();
    RuleFor(x => x.NewPrice).GreaterThan(0);
  }
}
