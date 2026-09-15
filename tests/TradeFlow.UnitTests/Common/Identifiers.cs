namespace TradeFlow.Domain.UnitTests.Common;

using TradeFlow.Domain.Common.Identifiers;
using Xunit;

public class ProductIdTests
{
  [Fact]
  public void New_ShouldGenerateNonEmptyId()
  {
    var id = ProductId.New();

    Assert.NotEqual(Guid.Empty, id.Value);
  }

  [Fact]
  public void Create_WithValidGuid_ShouldSucceed()
  {
    var guid = Guid.CreateVersion7();

    var result = ProductId.Create(guid);

    Assert.True(result.IsSuccess);
    Assert.Equal(guid, result.Value.Value);
  }

  [Fact]
  public void Create_WithEmptyGuid_ShouldFail()
  {
    var result = ProductId.Create(Guid.Empty);

    Assert.True(result.IsError);
  }

  [Fact]
  public void New_TwoIdsGeneratedInDifferentMilliseconds_ShouldBeSequential()
  {
    var first = ProductId.New();

    Thread.Sleep(2); // نضمن إن الـTimestamp (دقة ميلي ثانية) يختلف فعلاً

    var second = ProductId.New();

    var firstBytes = first.Value.ToByteArray(bigEndian: true);
    var secondBytes = second.Value.ToByteArray(bigEndian: true);

    Assert.True(
        new ReadOnlySpan<byte>(secondBytes)
            .SequenceCompareTo(firstBytes) >= 0);
  }

  [Fact]
  public void Equals_SameGuid_ShouldBeEqual()
  {
    var guid = Guid.CreateVersion7();
    var a = ProductId.Create(guid).Value;
    var b = ProductId.Create(guid).Value;

    Assert.Equal(a, b);
  }
}
