namespace TradeFlow.Domain.UnitTests.Common.ValueObjects;

using TradeFlow.Domain.Common.ValueObjects;
using Xunit;

public class DocumentNumberTests
{
  [Theory]
  [InlineData("ORD-1024")]
  [InlineData("INV-0042")]
  [InlineData("PO-9999")]
  public void Create_WithValidFormat_ShouldSucceed(string value)
  {
    var result = DocumentNumber.Create(value);

    Assert.True(result.IsSuccess);
    Assert.Equal(value, result.Value.Value);
  }

  [Fact]
  public void Create_WithLowercaseValue_ShouldNormalizeToUppercase()
  {
    var result = DocumentNumber.Create("ord-1024");

    Assert.True(result.IsSuccess);
    Assert.Equal("ORD-1024", result.Value.Value);
  }

  [Fact]
  public void Create_WithEmptyValue_ShouldFail()
  {
    var result = DocumentNumber.Create("");

    Assert.True(result.IsError);
    Assert.Equal(DocumentNumberErrors.Empty, result.TopError);
  }

  [Theory]
  [InlineData("ORD1024")]      // مفيش شرطة
  [InlineData("O-1024")]       // بادئة حرف واحد
  [InlineData("ORDERPO-1024")] // بادئة أطول من 5
  [InlineData("ORD-12")]       // أرقام أقل من 4
  [InlineData("ORD-ABCD")]     // مش أرقام
  [InlineData("123-1024")]     // بادئة أرقام مش حروف
  public void Create_WithInvalidFormat_ShouldFail(string value)
  {
    var result = DocumentNumber.Create(value);

    Assert.True(result.IsError);
    Assert.Equal(DocumentNumberErrors.InvalidFormat, result.TopError);
  }

  [Fact]
  public void Generate_WithValidPrefixAndSequence_ShouldSucceed()
  {
    var result = DocumentNumber.Generate("ORD", 24);

    Assert.True(result.IsSuccess);
    Assert.Equal("ORD-0024", result.Value.Value);
  }

  [Fact]
  public void Generate_WithLargeSequence_ShouldNotTruncate()
  {
    var result = DocumentNumber.Generate("ORD", 123456);

    Assert.True(result.IsSuccess);
    Assert.Equal("ORD-123456", result.Value.Value);
  }

  [Fact]
  public void Generate_WithLowercasePrefix_ShouldNormalizeToUppercase()
  {
    var result = DocumentNumber.Generate("ord", 5);

    Assert.True(result.IsSuccess);
    Assert.Equal("ORD-0005", result.Value.Value);
  }

  [Fact]
  public void Generate_WithEmptyPrefix_ShouldFail()
  {
    var result = DocumentNumber.Generate("", 5);

    Assert.True(result.IsError);
    Assert.Equal(DocumentNumberErrors.EmptyPrefix, result.TopError);
  }

  [Theory]
  [InlineData("O")]        // حرف واحد بس
  [InlineData("ORDERPO")]  // أطول من 5
  [InlineData("OR1")]      // فيه رقم
  public void Generate_WithInvalidPrefix_ShouldFail(string prefix)
  {
    var result = DocumentNumber.Generate(prefix, 5);

    Assert.True(result.IsError);
    Assert.Equal(DocumentNumberErrors.InvalidPrefix, result.TopError);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Generate_WithInvalidSequence_ShouldFail(long sequence)
  {
    var result = DocumentNumber.Generate("ORD", sequence);

    Assert.True(result.IsError);
    Assert.Equal(DocumentNumberErrors.InvalidSequence, result.TopError);
  }

  [Fact]
  public void Equals_SameValue_ShouldBeEqual()
  {
    var a = DocumentNumber.Create("ORD-1024").Value;
    var b = DocumentNumber.Generate("ORD", 1024).Value;

    Assert.Equal(a, b);
  }
}
