// Commands/CreateProduct/CreateProductCommandValidator.cs
namespace TradeFlow.Application.Inventory.Products.Commands.CreateProduct;

using FluentValidation;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
  public CreateProductCommandValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
    RuleFor(x => x.SellingPrice).GreaterThan(0);
    RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
    RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
    RuleFor(x => x.OpeningStockQuantity).GreaterThanOrEqualTo(0);
  }
}
