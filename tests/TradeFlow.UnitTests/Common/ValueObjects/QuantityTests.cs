namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class QuantityTests
{
  [Fact]
  public void Create_WithPositiveValue_ShouldSucceed()
  {
    var result = Quantity.Create(5);

    Assert.True(result.IsSuccess);
    Assert.Equal(5, result.Value.Value);
  }

  [Fact]
  public void Create_WithZero_ShouldFail()
  {
    var result = Quantity.Create(0);

    Assert.True(result.IsError);
    Assert.Equal(QuantityErrors.MustBePositive, result.TopError);
  }

  [Fact]
  public void Create_WithNegativeValue_ShouldFail()
  {
    var result = Quantity.Create(-3);

    Assert.True(result.IsError);
    Assert.Equal(QuantityErrors.MustBePositive, result.TopError);
  }

  [Fact]
  public void Zero_ShouldHaveValueZero()
  {
    var quantity = Quantity.Zero();

    Assert.Equal(0, quantity.Value);
  }

  [Fact]
  public void Add_TwoQuantities_ShouldReturnSum()
  {
    var a = Quantity.Create(10).Value;
    var b = Quantity.Create(5).Value;

    var result = a.Add(b);

    Assert.True(result.IsSuccess);
    Assert.Equal(15, result.Value.Value);
  }

  [Fact]
  public void Subtract_ValidValue_ShouldReturnDifference()
  {
    var a = Quantity.Create(10).Value;
    var b = Quantity.Create(4).Value;

    var result = a.Subtract(b);

    Assert.True(result.IsSuccess);
    Assert.Equal(6, result.Value.Value);
  }

  [Fact]
  public void Subtract_ResultingInNegative_ShouldFail()
  {
    var a = Quantity.Create(3).Value;
    var b = Quantity.Create(10).Value;

    var result = a.Subtract(b);

    Assert.True(result.IsError);
    Assert.Equal(QuantityErrors.ResultCannotBeNegative, result.TopError);
  }

  [Fact]
  public void Subtract_ResultingInZero_ShouldSucceed()
  {
    var a = Quantity.Create(5).Value;
    var b = Quantity.Create(5).Value;

    var result = a.Subtract(b);

    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Value.Value);
  }

  [Fact]
  public void IsGreaterThan_WhenGreater_ShouldReturnTrue()
  {
    var a = Quantity.Create(10).Value;
    var b = Quantity.Create(5).Value;

    Assert.True(a.IsGreaterThan(b));
  }

  [Fact]
  public void IsGreaterThanOrEqual_WhenEqual_ShouldReturnTrue()
  {
    var a = Quantity.Create(5).Value;
    var b = Quantity.Create(5).Value;

    Assert.True(a.IsGreaterThanOrEqual(b));
  }

  [Fact]
  public void Equals_SameValue_ShouldBeEqual()
  {
    var a = Quantity.Create(7).Value;
    var b = Quantity.Create(7).Value;

    Assert.Equal(a, b);
  }
}
