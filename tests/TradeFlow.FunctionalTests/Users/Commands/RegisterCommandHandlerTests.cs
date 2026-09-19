namespace TradeFlow.Application.UnitTests.Users.Commands;

using Moq;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Application.Users.Commands.Register;
using TradeFlow.Domain.Common.Results;
using Xunit;

public class RegisterCommandHandlerTests
{
  private readonly Mock<IIdentityService> _identityServiceMock = new();

  [Fact]
  public async Task Handle_ShouldCallIdentityServiceRegisterAsync_WithCorrectArguments()
  {
    var expected = new AuthResult(
        "access-token",
        "refresh-token",
        DateTimeOffset.UtcNow.AddMinutes(15),
        DateTimeOffset.UtcNow.AddDays(7));

    _identityServiceMock
        .Setup(s => s.RegisterAsync("ACME", "Ahmed Ali", "ahmed@test.com", "P@ssw0rd1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    var handler = new RegisterCommandHandler(_identityServiceMock.Object);
    var command = new RegisterCommand("ACME", "Ahmed Ali", "ahmed@test.com", "P@ssw0rd1");

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
    _identityServiceMock.Verify(
        s => s.RegisterAsync("ACME", "Ahmed Ali", "ahmed@test.com", "P@ssw0rd1", It.IsAny<CancellationToken>()),
        Times.Once);
  }

  [Fact]
  public async Task Handle_WhenIdentityServiceFails_ShouldReturnError()
  {
    _identityServiceMock
        .Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Domain.Users.UserErrors.EmailAlreadyExists);

    var handler = new RegisterCommandHandler(_identityServiceMock.Object);
    var command = new RegisterCommand("ACME", "Ahmed Ali", "existing@test.com", "P@ssw0rd1");

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(Domain.Users.UserErrors.EmailAlreadyExists, result.TopError);
  }
}
