namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class MoneyTests
{
  [Fact]
  public void EGP_WithValidAmount_ShouldSucceed()
  {
    var result = Money.EGP(100);

    Assert.True(result.IsSuccess);
    Assert.Equal(100, result.Value.Amount);
    Assert.Equal("EGP", result.Value.Currency);
  }

  [Fact]
  public void EGP_WithNegativeAmount_ShouldFailWithValidationError()
  {
    var result = Money.EGP(-10);

    Assert.True(result.IsError);
    Assert.Equal(MoneyErrors.NegativeAmount, result.TopError);
  }

  [Fact]
  public void EGP_WithZeroAmount_ShouldSucceed()
  {
    var result = Money.EGP(0);

    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Value.Amount);
  }

  [Fact]
  public void Add_TwoMoneyValues_ShouldReturnSum()
  {
    var a = Money.EGP(100).Value;
    var b = Money.EGP(50).Value;

    var result = a.Add(b);

    Assert.True(result.IsSuccess);
    Assert.Equal(150, result.Value.Amount);
  }

  [Fact]
  public void Subtract_ValidAmount_ShouldReturnDifference()
  {
    var a = Money.EGP(100).Value;
    var b = Money.EGP(30).Value;

    var result = a.Subtract(b);

    Assert.True(result.IsSuccess);
    Assert.Equal(70, result.Value.Amount);
  }

  [Fact]
  public void Subtract_ResultingInNegative_ShouldFail()
  {
    var a = Money.EGP(30).Value;
    var b = Money.EGP(100).Value;

    var result = a.Subtract(b);

    Assert.True(result.IsError);
    Assert.Equal(MoneyErrors.NegativeResult, result.TopError);
  }

  [Fact]
  public void Multiply_ByPositiveFactor_ShouldReturnProduct()
  {
    var unitPrice = Money.EGP(50).Value;

    var result = unitPrice.Multiply(3);

    Assert.True(result.IsSuccess);
    Assert.Equal(150, result.Value.Amount);
  }

  [Fact]
  public void Multiply_ByNegativeFactor_ShouldFail()
  {
    var unitPrice = Money.EGP(50).Value;

    var result = unitPrice.Multiply(-2);

    Assert.True(result.IsError);
    Assert.Equal(MoneyErrors.NegativeFactor, result.TopError);
  }

  [Fact]
  public void Equals_SameAmountAndCurrency_ShouldBeEqual()
  {
    var a = Money.EGP(100).Value;
    var b = Money.EGP(100).Value;

    Assert.Equal(a, b);
    Assert.True(a == b);
  }

  [Fact]
  public void Zero_ShouldHaveAmountZero()
  {
    var zero = Money.Zero();

    Assert.Equal(0, zero.Amount);
  }
}
