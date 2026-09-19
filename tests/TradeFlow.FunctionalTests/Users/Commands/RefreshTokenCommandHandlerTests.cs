namespace TradeFlow.Application.UnitTests.Users.Commands;

using Moq;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Users.Commands.RefreshToken;
using TradeFlow.Domain.Users;
using Xunit;

public class RefreshTokenCommandHandlerTests
{
  private readonly Mock<IIdentityService> _identityServiceMock = new();

  [Fact]
  public async Task Handle_WithValidTokens_ShouldReturnNewAuthResult()
  {
    var expected = new AuthResult(
        "new-access",
        "new-refresh",
        DateTimeOffset.UtcNow.AddMinutes(15),
        DateTimeOffset.UtcNow.AddDays(7));

    _identityServiceMock
        .Setup(s => s.RefreshAsync("old-access", "old-refresh", It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var handler = new RefreshTokenCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new RefreshTokenCommand("old-access", "old-refresh"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public async Task Handle_WithInvalidRefreshToken_ShouldReturnError()
  {
    _identityServiceMock
        .Setup(s => s.RefreshAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(UserErrors.RefreshTokenNotFound);

    var handler = new RefreshTokenCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new RefreshTokenCommand("access", "bad-token"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.RefreshTokenNotFound, result.TopError);
  }
}
