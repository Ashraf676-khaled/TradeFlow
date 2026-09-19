namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Commands;

using TradeFlow.Application.Inventory.Warehouses.Commands.ChangeWarehouseLocation;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class ChangeWarehouseLocationCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidLocation_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var handler = new ChangeWarehouseLocationCommandHandler(context);
    var result = await handler.Handle(new ChangeWarehouseLocationCommand(created.Value, "Alexandria"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("Alexandria", context.Warehouses.Single().Location);
  }

  [Fact]
  public async Task Handle_WithNonExistentWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new ChangeWarehouseLocationCommandHandler(context);

    var result = await handler.Handle(new ChangeWarehouseLocationCommand(Guid.NewGuid(), "Alexandria"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotFound, result.TopError);
  }
}
