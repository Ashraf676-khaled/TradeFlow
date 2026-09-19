namespace TradeFlow.Application.UnitTests.Users.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Users.Commands.RevokeToken;
using Xunit;

public class RevokeTokenCommandValidatorTests
{
  private readonly RevokeTokenCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenRefreshTokenIsEmpty()
  {
    var result = _validator.TestValidate(new RevokeTokenCommand(""));
    result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
  }

  [Fact]
  public void Should_NotHaveError_WhenRefreshTokenIsProvided()
  {
    var result = _validator.TestValidate(new RevokeTokenCommand("some-token"));
    result.ShouldNotHaveAnyValidationErrors();
  }
}
