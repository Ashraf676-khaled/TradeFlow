namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Commands;

using TradeFlow.Application.Inventory.Warehouses.Commands.ActivateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class ActivateWarehouseCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithInactiveWarehouse_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var deactivateHandler = new DeactivateWarehouseCommandHandler(context);
    await deactivateHandler.Handle(new DeactivateWarehouseCommand(created.Value), CancellationToken.None);

    var handler = new ActivateWarehouseCommandHandler(context);
    var result = await handler.Handle(new ActivateWarehouseCommand(created.Value), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(context.Warehouses.Single().IsActive);
  }

  [Fact]
  public async Task Handle_WithAlreadyActiveWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var handler = new ActivateWarehouseCommandHandler(context);
    var result = await handler.Handle(new ActivateWarehouseCommand(created.Value), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.AlreadyActive, result.TopError);
  }

  [Fact]
  public async Task Handle_WithNonExistentWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new ActivateWarehouseCommandHandler(context);

    var result = await handler.Handle(new ActivateWarehouseCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotFound, result.TopError);
  }
}
