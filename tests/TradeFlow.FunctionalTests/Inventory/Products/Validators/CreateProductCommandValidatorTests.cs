namespace TradeFlow.Application.UnitTests.Inventory.Products.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using Xunit;

public class CreateProductCommandValidatorTests
{
  private readonly CreateProductCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenNameIsEmpty()
  {
    var result = _validator.TestValidate(new CreateProductCommand("", "SKU1", 100, 50, 5));
    result.ShouldHaveValidationErrorFor(x => x.Name);
  }

  [Fact]
  public void Should_HaveError_WhenSkuIsEmpty()
  {
    var result = _validator.TestValidate(new CreateProductCommand("Door", "", 100, 50, 5));
    result.ShouldHaveValidationErrorFor(x => x.Sku);
  }

  [Fact]
  public void Should_HaveError_WhenSellingPriceIsZeroOrNegative()
  {
    var result = _validator.TestValidate(new CreateProductCommand("Door", "SKU1", 0, 50, 5));
    result.ShouldHaveValidationErrorFor(x => x.SellingPrice);
  }

  [Fact]
  public void Should_HaveError_WhenCostIsNegative()
  {
    var result = _validator.TestValidate(new CreateProductCommand("Door", "SKU1", 100, -1, 5));
    result.ShouldHaveValidationErrorFor(x => x.Cost);
  }

  [Fact]
  public void Should_HaveError_WhenMinimumStockIsNegative()
  {
    var result = _validator.TestValidate(new CreateProductCommand("Door", "SKU1", 100, 50, -1));
    result.ShouldHaveValidationErrorFor(x => x.MinimumStock);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenAllFieldsAreValid()
  {
    var result = _validator.TestValidate(new CreateProductCommand("Door", "SKU1", 100, 50, 5));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
