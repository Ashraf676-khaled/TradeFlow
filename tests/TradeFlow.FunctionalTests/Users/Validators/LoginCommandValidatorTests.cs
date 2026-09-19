namespace TradeFlow.Application.UnitTests.Users.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Users.Commands.Login;
using Xunit;

public class LoginCommandValidatorTests
{
  private readonly LoginCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenEmailIsEmpty()
  {
    var result = _validator.TestValidate(new LoginCommand("", "password"));
    result.ShouldHaveValidationErrorFor(x => x.Email);
  }

  [Fact]
  public void Should_HaveError_WhenPasswordIsEmpty()
  {
    var result = _validator.TestValidate(new LoginCommand("ahmed@test.com", ""));
    result.ShouldHaveValidationErrorFor(x => x.Password);
  }

  [Fact]
  public void Should_NotHaveAnyErrors_WhenValid()
  {
    var result = _validator.TestValidate(new LoginCommand("ahmed@test.com", "anyPassword"));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
