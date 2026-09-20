namespace TradeFlow.Application.UnitTests.Purchasing.PurchaseOrders.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;
using Xunit;

public class CreatePurchaseOrderCommandValidatorTests
{
  private readonly CreatePurchaseOrderCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenItemsIsEmpty()
  {
    var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(), []);
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Items);
  }

  [Fact]
  public void Should_HaveError_WhenItemQuantityIsZero()
  {
    var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(),
        [new PurchaseOrderItemInput(Guid.NewGuid(), 0, 50)]);
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor("Items[0].Quantity");
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenValid()
  {
    var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), Guid.NewGuid(),
        [new PurchaseOrderItemInput(Guid.NewGuid(), 5, 50)]);
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }
}
