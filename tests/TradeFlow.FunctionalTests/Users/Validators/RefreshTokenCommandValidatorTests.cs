namespace TradeFlow.Application.UnitTests.Users.Validators;

using FluentValidation.TestHelper;
using TradeFlow.Application.Users.Commands.RefreshToken;
using Xunit;

public class RefreshTokenCommandValidatorTests
{
  private readonly RefreshTokenCommandValidator _validator = new();

  [Fact]
  public void Should_HaveError_WhenAccessTokenIsEmpty()
  {
    var result = _validator.TestValidate(new RefreshTokenCommand("", "refresh"));
    result.ShouldHaveValidationErrorFor(x => x.AccessToken);
  }

  [Fact]
  public void Should_HaveError_WhenRefreshTokenIsEmpty()
  {
    var result = _validator.TestValidate(new RefreshTokenCommand("access", ""));
    result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
  }
}
