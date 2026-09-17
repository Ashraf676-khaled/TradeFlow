namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class PhoneNumberTests
{
  [Theory]
  [InlineData("01012345678")]
  [InlineData("01112345678")]
  [InlineData("01212345678")]
  [InlineData("01512345678")]
  public void Create_WithValidEgyptianNumber_ShouldSucceed(string number)
  {
    var result = PhoneNumber.Create(number);

    Assert.True(result.IsSuccess);
    Assert.Equal(number, result.Value.Value);
  }

  [Fact]
  public void Create_WithSpacesAndDashes_ShouldNormalize()
  {
    var result = PhoneNumber.Create("010-1234-5678");

    Assert.True(result.IsSuccess);
    Assert.Equal("01012345678", result.Value.Value);
  }

  [Fact]
  public void Create_WithEmptyValue_ShouldFail()
  {
    var result = PhoneNumber.Create("");

    Assert.True(result.IsError);
    Assert.Equal(PhoneNumberErrors.Empty, result.TopError);
  }

  [Fact]
  public void Create_WithNullValue_ShouldFail()
  {
    var result = PhoneNumber.Create(null!);

    Assert.True(result.IsError);
    Assert.Equal(PhoneNumberErrors.Empty, result.TopError);
  }

  [Theory]
  [InlineData("01312345678")] // بادئة مش موجودة (013)
  [InlineData("0101234567")]  // 10 أرقام بس
  [InlineData("010123456789")] // 12 رقم
  [InlineData("02012345678")] // مش بادئة موبايل
  [InlineData("abc12345678")] // حروف
  public void Create_WithInvalidFormat_ShouldFail(string number)
  {
    var result = PhoneNumber.Create(number);

    Assert.True(result.IsError);
    Assert.Equal(PhoneNumberErrors.InvalidFormat, result.TopError);
  }

  [Fact]
  public void Equals_SameNormalizedValue_ShouldBeEqual()
  {
    var a = PhoneNumber.Create("01012345678").Value;
    var b = PhoneNumber.Create("010-1234-5678").Value;

    Assert.Equal(a, b);
  }
}
