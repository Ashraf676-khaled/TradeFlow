namespace TradeFlow.Application.UnitTests.Sales.SalesOrders.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using Xunit;

public class CreateSalesOrderCommandValidatorTests
{
  private readonly CreateSalesOrderCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenItemsIsEmpty()
  {
    var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(), []);
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Items);
  }

  [Fact]
  public void Should_HaveError_WhenItemQuantityIsZero()
  {
    var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(),
        [new SalesOrderItemInput(Guid.NewGuid(), 0, 100)]);
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor("Items[0].Quantity");
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenValid()
  {
    var command = new CreateSalesOrderCommand(Guid.NewGuid(), Guid.NewGuid(),
        [new SalesOrderItemInput(Guid.NewGuid(), 5, 100)]);
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }
}
