namespace TradeFlow.Application.UnitTests.Customers.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Customers.Commands.CreateCustomer;
using Xunit;

public class CreateCustomerCommandValidatorTests
{
  private readonly CreateCustomerCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenNameIsEmpty()
  {
    var result = _validator.TestValidate(new CreateCustomerCommand("", "01012345678", 5000));
    result.ShouldHaveValidationErrorFor(x => x.Name);
  }

  [Fact]
  public void Should_HaveError_WhenPhoneIsEmpty()
  {
    var result = _validator.TestValidate(new CreateCustomerCommand("Ahmed", "", 5000));
    result.ShouldHaveValidationErrorFor(x => x.Phone);
  }

  [Fact]
  public void Should_HaveError_WhenCreditLimitIsNegative()
  {
    var result = _validator.TestValidate(new CreateCustomerCommand("Ahmed", "01012345678", -1));
    result.ShouldHaveValidationErrorFor(x => x.CreditLimit);
  }

  [Fact]
  public void Should_HaveError_WhenEmailIsInvalid()
  {
    var result = _validator.TestValidate(new CreateCustomerCommand("Ahmed", "01012345678", 5000, "bad-email"));
    result.ShouldHaveValidationErrorFor(x => x.Email);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenEmailIsNull()
  {
    var result = _validator.TestValidate(new CreateCustomerCommand("Ahmed", "01012345678", 5000));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
