namespace TradeFlow.Application.UnitTests.Customers.Commands;

using TradeFlow.Application.Customers.Commands.ChangeCreditLimit;
using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Customers;
using Xunit;

public class ChangeCreditLimitCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidLimit_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;

    var handler = new ChangeCreditLimitCommandHandler(context);
    var result = await handler.Handle(new ChangeCreditLimitCommand(customerId, 10000), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_ForNonExistingCustomer_ShouldReturnNotFound()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new ChangeCreditLimitCommandHandler(context);

    var result = await handler.Handle(new ChangeCreditLimitCommand(Guid.NewGuid(), 10000), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.NotFound, result.TopError);
  }
}
