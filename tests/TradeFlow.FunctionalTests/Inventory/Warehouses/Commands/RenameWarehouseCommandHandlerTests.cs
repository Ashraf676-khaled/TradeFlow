namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Commands;

using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.RenameWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class RenameWarehouseCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  private async Task<Guid> CreateWarehouseAsync(TradeFlow.Infrastructure.Data.AppDbContext context, string name = "Main Warehouse")
  {
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var result = await createHandler.Handle(new CreateWarehouseCommand(name, "Cairo"), CancellationToken.None);
    return result.Value;
  }

  [Fact]
  public async Task Handle_WithValidName_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var warehouseId = await CreateWarehouseAsync(context);
    var handler = new RenameWarehouseCommandHandler(context);

    var result = await handler.Handle(new RenameWarehouseCommand(warehouseId, "New Name"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("New Name", context.Warehouses.Single().Name);
  }

  [Fact]
  public async Task Handle_WithNonExistentWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new RenameWarehouseCommandHandler(context);

    var result = await handler.Handle(new RenameWarehouseCommand(Guid.NewGuid(), "New Name"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotFound, result.TopError);
  }

  [Fact]
  public async Task Handle_WithDuplicateName_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var firstId = await CreateWarehouseAsync(context, "Warehouse A");
    var secondId = await CreateWarehouseAsync(context, "Warehouse B");
    var handler = new RenameWarehouseCommandHandler(context);

    var result = await handler.Handle(new RenameWarehouseCommand(secondId, "Warehouse A"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NameAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_RenamingToSameName_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var warehouseId = await CreateWarehouseAsync(context, "Warehouse A");
    var handler = new RenameWarehouseCommandHandler(context);

    var result = await handler.Handle(new RenameWarehouseCommand(warehouseId, "Warehouse A"), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }
}
