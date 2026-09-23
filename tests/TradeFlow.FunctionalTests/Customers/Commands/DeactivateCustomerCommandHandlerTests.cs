namespace TradeFlow.Application.UnitTests.Customers.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Customers.Commands.DeactivateCustomer;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Customers;
using Xunit;

public class DeactivateCustomerCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_ForActiveCustomer_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;

    var handler = new DeactivateCustomerCommandHandler(context);
    var result = await handler.Handle(new DeactivateCustomerCommand(customerId), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_ForAlreadyInactiveCustomer_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;

    var handler = new DeactivateCustomerCommandHandler(context);
    await handler.Handle(new DeactivateCustomerCommand(customerId), CancellationToken.None);
    var result = await handler.Handle(new DeactivateCustomerCommand(customerId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.AlreadyInactive, result.TopError);
  }
}
