namespace TradeFlow.Application.UnitTests.Purchasing.Suppliers.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using Xunit;

public class CreateSupplierCommandValidatorTests
{
  private readonly CreateSupplierCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenNameIsEmpty()
  {
    var result = _validator.TestValidate(new CreateSupplierCommand("", "01012345678"));
    result.ShouldHaveValidationErrorFor(x => x.Name);
  }

  [Fact]
  public void Should_HaveError_WhenPhoneIsEmpty()
  {
    var result = _validator.TestValidate(new CreateSupplierCommand("شركة الأمل", ""));
    result.ShouldHaveValidationErrorFor(x => x.Phone);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenValid()
  {
    var result = _validator.TestValidate(new CreateSupplierCommand("شركة الأمل", "01012345678"));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
