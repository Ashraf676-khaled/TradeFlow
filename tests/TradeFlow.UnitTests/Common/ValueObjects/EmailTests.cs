namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class EmailTests
{
  [Theory]
  [InlineData("user@example.com")]
  [InlineData("user.name@example.com")]
  [InlineData("user+tag@example.co.uk")]
  [InlineData("user123@sub.example.com")]
  public void Create_WithValidEmail_ShouldSucceed(string email)
  {
    var result = Email.Create(email);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_WithUppercaseEmail_ShouldNormalizeToLowercase()
  {
    var result = Email.Create("User@Example.COM");

    Assert.True(result.IsSuccess);
    Assert.Equal("user@example.com", result.Value.Value);
  }

  [Fact]
  public void Create_WithLeadingOrTrailingSpaces_ShouldTrim()
  {
    var result = Email.Create("  user@example.com  ");

    Assert.True(result.IsSuccess);
    Assert.Equal("user@example.com", result.Value.Value);
  }

  [Fact]
  public void Create_WithEmptyValue_ShouldFail()
  {
    var result = Email.Create("");

    Assert.True(result.IsError);
    Assert.Equal(EmailErrors.Empty, result.TopError);
  }

  [Fact]
  public void Create_WithNullValue_ShouldFail()
  {
    var result = Email.Create(null!);

    Assert.True(result.IsError);
    Assert.Equal(EmailErrors.Empty, result.TopError);
  }

  [Theory]
  [InlineData("notanemail")]
  [InlineData("missing@domain")]
  [InlineData("@example.com")]
  [InlineData("user@")]
  [InlineData("user @example.com")]
  [InlineData("user@ example.com")]
  public void Create_WithInvalidFormat_ShouldFail(string email)
  {
    var result = Email.Create(email);

    Assert.True(result.IsError);
    Assert.Equal(EmailErrors.InvalidFormat, result.TopError);
  }

  [Fact]
  public void Create_ExceedingMaxLength_ShouldFail()
  {
    var longEmail = new string('a', 250) + "@x.com";

    var result = Email.Create(longEmail);

    Assert.True(result.IsError);
    Assert.Equal(EmailErrors.TooLong, result.TopError);
  }

  [Fact]
  public void Equals_SameValueDifferentCase_ShouldBeEqual()
  {
    var a = Email.Create("User@Example.com").Value;
    var b = Email.Create("user@example.com").Value;

    Assert.Equal(a, b);
  }
}
