namespace TradeFlow.Application.UnitTests.Users.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Users.Commands.Register;
using Xunit;

public class RegisterCommandValidatorTests
{
  private readonly RegisterCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenCompanyNameIsEmpty()
  {
    var command = new RegisterCommand("", "Ahmed", "ahmed@test.com", "P@ssw0rd1");
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.CompanyName);
  }

  [Fact]
  public void Should_HaveError_WhenEmailIsInvalid()
  {
    var command = new RegisterCommand("ACME", "Ahmed", "not-an-email", "P@ssw0rd1");
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Email);
  }

  [Theory]
  [InlineData("short1")]      // أقل من 8
  [InlineData("nouppercase1")]// من غير حرف كبير
  [InlineData("NoDigitsHere")]// من غير رقم
  public void Should_HaveError_WhenPasswordDoesNotMeetRules(string password)
  {
    var command = new RegisterCommand("ACME", "Ahmed", "ahmed@test.com", password);
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Password);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenAllFieldsAreValid()
  {
    var command = new RegisterCommand("ACME", "Ahmed Ali", "ahmed@test.com", "P@ssw0rd1");
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }
}
