namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Queries;

using TradeFlow.Application.Inventory.Warehouses.Commands.ActivateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses;
using TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouses;
using TradeFlow.Application.UnitTests.Helpers;
using Xunit;

public class GetWarehousesQueryHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithMultipleWarehouses_ShouldReturnAllOrderedByName()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    await createHandler.Handle(new CreateWarehouseCommand("Zeta Warehouse", "Cairo"), CancellationToken.None);
    await createHandler.Handle(new CreateWarehouseCommand("Alpha Warehouse", "Giza"), CancellationToken.None);

    var mapper = TestMapperFactory.Create();
    var handler = new GetWarehousesQueryHandler(context, mapper, _currentUser);

    var result = await handler.Handle(new GetWarehousesQuery(), CancellationToken.None);

    Assert.Equal(2, result.Items.Count);
    Assert.Equal("Alpha Warehouse", result.Items[0].Name);
    Assert.Equal("Zeta Warehouse", result.Items[1].Name);
  }

  [Fact]
  public async Task Handle_WithIsActiveFilter_ShouldReturnOnlyMatching()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var active = await createHandler.Handle(new CreateWarehouseCommand("Active Warehouse", "Cairo"), CancellationToken.None);
    var toDeactivate = await createHandler.Handle(new CreateWarehouseCommand("Inactive Warehouse", "Giza"), CancellationToken.None);

    var deactivateHandler = new DeactivateWarehouseCommandHandler(context);
    await deactivateHandler.Handle(new DeactivateWarehouseCommand(toDeactivate.Value), CancellationToken.None);

    var mapper = TestMapperFactory.Create();
    var handler = new GetWarehousesQueryHandler(context, mapper, _currentUser);

    var result = await handler.Handle(new GetWarehousesQuery(IsActive: true), CancellationToken.None);

    Assert.Single(result.Items);
    Assert.Equal("Active Warehouse", result.Items[0].Name);
  }

  [Fact]
  public async Task Handle_WithNoWarehouses_ShouldSeedAndReturnDefaultWarehouse()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var mapper = TestMapperFactory.Create();
    var handler = new GetWarehousesQueryHandler(context, mapper, _currentUser);

    var result = await handler.Handle(new GetWarehousesQuery(), CancellationToken.None);

    // A default warehouse is seeded lazily so dropdowns are never empty.
    var warehouse = Assert.Single(result.Items);
    Assert.Equal(DefaultWarehouse.DefaultName, warehouse.Name);
    Assert.True(warehouse.IsActive);
  }
}
