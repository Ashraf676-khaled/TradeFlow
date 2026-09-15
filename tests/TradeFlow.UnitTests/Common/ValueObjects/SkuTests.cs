namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class SkuTests
{
  [Fact]
  public void Create_WithValidValue_ShouldSucceed()
  {
    var result = Sku.Create("PRD-1024");

    Assert.True(result.IsSuccess);
    Assert.Equal("PRD-1024", result.Value.Value);
  }

  [Fact]
  public void Create_WithLowercaseValue_ShouldNormalizeToUppercase()
  {
    var result = Sku.Create("prd-1024");

    Assert.True(result.IsSuccess);
    Assert.Equal("PRD-1024", result.Value.Value);
  }

  [Fact]
  public void Create_WithLeadingOrTrailingSpaces_ShouldTrim()
  {
    var result = Sku.Create("  PRD-1024  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("PRD-1024", result.Value.Value);
  }

  [Fact]
  public void Create_WithEmptyValue_ShouldFail()
  {
    var result = Sku.Create("");

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.Empty, result.TopError);
  }

  [Fact]
  public void Create_WithWhitespaceOnly_ShouldFail()
  {
    var result = Sku.Create("   ");

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.Empty, result.TopError);
  }

  [Fact]
  public void Create_WithNullValue_ShouldFail()
  {
    var result = Sku.Create(null!);

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.Empty, result.TopError);
  }

  [Fact]
  public void Create_ExceedingMaxLength_ShouldFail()
  {
    var longValue = new string('A', 51);

    var result = Sku.Create(longValue);

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.TooLong, result.TopError);
  }

  [Fact]
  public void Create_AtMaxLength_ShouldSucceed()
  {
    var value = new string('A', 50);

    var result = Sku.Create(value);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_WithInvalidCharacters_ShouldFail()
  {
    var result = Sku.Create("PRD_1024#");

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.InvalidFormat, result.TopError);
  }

  [Fact]
  public void Create_WithSpacesInMiddle_ShouldFail()
  {
    var result = Sku.Create("PRD 1024");

    Assert.True(result.IsError);
    Assert.Equal(SkuErrors.InvalidFormat, result.TopError);
  }

  [Fact]
  public void Equals_SameValueDifferentCase_ShouldBeEqual()
  {
    var a = Sku.Create("prd-1024").Value;
    var b = Sku.Create("PRD-1024").Value;

    Assert.Equal(a, b);
  }
}
