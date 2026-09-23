namespace TradeFlow.Application.UnitTests.Customers.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Customers;
using Xunit;

public class CreateCustomerCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateCustomerCommandHandler(context, _currentUser);

    var result = await handler.Handle(
        new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.Customers);
  }

  [Fact]
  public async Task Handle_WithDuplicatePhone_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateCustomerCommandHandler(context, _currentUser);

    await handler.Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None);
    var result = await handler.Handle(new CreateCustomerCommand("Another Co", "01012345678", 3000), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.PhoneAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_WithInvalidEmail_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateCustomerCommandHandler(context, _currentUser);

    var result = await handler.Handle(
        new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000, "not-an-email"), CancellationToken.None);

    Assert.True(result.IsError);
  }

  [Fact]
  public async Task Handle_WithoutTenant_ShouldFail()
  {
    var noTenantUser = new FakeCurrentUserService { TenantId = null };
    await using var context = TestDbContextFactory.Create(noTenantUser);
    var handler = new CreateCustomerCommandHandler(context, noTenantUser);

    var result = await handler.Handle(
        new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None);

    Assert.True(result.IsError);
  }
}
