namespace TradeFlow.Application.UnitTests.Inventory.Products.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Inventory.Products.Commands.ChangeSellingPrice;
using Xunit;

public class ChangeSellingPriceCommandValidatorTests
{
  private readonly ChangeSellingPriceCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenProductIdIsEmpty()
  {
    var result = _validator.TestValidate(new ChangeSellingPriceCommand(Guid.Empty, 100));
    result.ShouldHaveValidationErrorFor(x => x.ProductId);
  }

  [Fact]
  public void Should_HaveError_WhenNewPriceIsZeroOrNegative()
  {
    var result = _validator.TestValidate(new ChangeSellingPriceCommand(Guid.NewGuid(), 0));
    result.ShouldHaveValidationErrorFor(x => x.NewPrice);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenValid()
  {
    var result = _validator.TestValidate(new ChangeSellingPriceCommand(Guid.NewGuid(), 150));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
