namespace TradeFlow.Domain.Tests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class DiscountTests
{
  [Fact]
  public void Create_WithValidPercentage_ShouldSucceed()
  {
    var result = Discount.Create(15);

    Assert.True(result.IsSuccess);
    Assert.Equal(15, result.Value.Percentage);
  }

  [Fact]
  public void Create_WithZeroPercentage_ShouldSucceed()
  {
    var result = Discount.Create(0);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_WithHundredPercentage_ShouldSucceed()
  {
    var result = Discount.Create(100);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_WithNegativePercentage_ShouldFail()
  {
    var result = Discount.Create(-5);

    Assert.True(result.IsError);
    Assert.Equal(DiscountErrors.OutOfRange, result.TopError);
  }

  [Fact]
  public void Create_WithPercentageAboveHundred_ShouldFail()
  {
    var result = Discount.Create(101);

    Assert.True(result.IsError);
    Assert.Equal(DiscountErrors.OutOfRange, result.TopError);
  }

  [Fact]
  public void None_ShouldHaveZeroPercentage()
  {
    var discount = Discount.None();

    Assert.Equal(0, discount.Percentage);
  }

  [Fact]
  public void ApplyTo_TenPercentDiscount_ShouldReduceAmountCorrectly()
  {
    var discount = Discount.Create(10).Value;
    var amount = Money.EGP(200).Value;

    var result = discount.ApplyTo(amount);

    Assert.True(result.IsSuccess);
    Assert.Equal(180, result.Value.Amount);
  }

  [Fact]
  public void ApplyTo_ZeroDiscount_ShouldReturnSameAmount()
  {
    var discount = Discount.None();
    var amount = Money.EGP(500).Value;

    var result = discount.ApplyTo(amount);

    Assert.True(result.IsSuccess);
    Assert.Equal(500, result.Value.Amount);
  }

  [Fact]
  public void ApplyTo_HundredPercentDiscount_ShouldReturnZero()
  {
    var discount = Discount.Create(100).Value;
    var amount = Money.EGP(300).Value;

    var result = discount.ApplyTo(amount);

    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Value.Amount);
  }

  [Fact]
  public void Equals_SamePercentage_ShouldBeEqual()
  {
    var a = Discount.Create(20).Value;
    var b = Discount.Create(20).Value;

    Assert.Equal(a, b);
  }
}
