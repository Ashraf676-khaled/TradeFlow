namespace TradeFlow.Application.UnitTests.Users.Commands;

using Moq;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Users.Commands.Login;
using TradeFlow.Domain.Users;
using Xunit;

public class LoginCommandHandlerTests
{
  private readonly Mock<IIdentityService> _identityServiceMock = new();

  [Fact]
  public async Task Handle_WithValidCredentials_ShouldReturnAuthResult()
  {
    var expected = new AuthResult(
        "access-token",
        "refresh-token",
        DateTimeOffset.UtcNow.AddMinutes(15),
        DateTimeOffset.UtcNow.AddDays(7));

    _identityServiceMock
        .Setup(s => s.LoginAsync("ahmed@test.com", "P@ssw0rd1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var handler = new LoginCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new LoginCommand("ahmed@test.com", "P@ssw0rd1"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public async Task Handle_WithInvalidCredentials_ShouldReturnNotFound()
  {
    _identityServiceMock
        .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(UserErrors.NotFound);

    var handler = new LoginCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new LoginCommand("wrong@test.com", "wrongpass"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.NotFound, result.TopError);
  }

  [Fact]
  public async Task Handle_ForInactiveUser_ShouldReturnInactiveUserError()
  {
    _identityServiceMock
        .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(UserErrors.InactiveUser);

    var handler = new LoginCommandHandler(_identityServiceMock.Object);
    var result = await handler.Handle(new LoginCommand("inactive@test.com", "P@ssw0rd1"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.InactiveUser, result.TopError);
  }
}
