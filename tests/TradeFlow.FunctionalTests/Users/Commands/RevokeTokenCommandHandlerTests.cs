namespace TradeFlow.Application.UnitTests.Users.Commands;

using Moq;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Users.Commands.RevokeToken;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Users;
using Xunit;

public class RevokeTokenCommandHandlerTests
{
  private readonly Mock<IIdentityService> _identityServiceMock = new();

  [Fact]
  public async Task Handle_WithValidToken_ShouldSucceed()
  {
    _identityServiceMock
        .Setup(s => s.RevokeAsync("valid-token", It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success);

    var handler = new RevokeTokenCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new RevokeTokenCommand("valid-token"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    _identityServiceMock.Verify(s => s.RevokeAsync("valid-token", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_WithNonExistingToken_ShouldReturnNotFound()
  {
    _identityServiceMock
        .Setup(s => s.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(UserErrors.RefreshTokenNotFound);

    var handler = new RevokeTokenCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new RevokeTokenCommand("nonexistent"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.RefreshTokenNotFound, result.TopError);
  }
}
