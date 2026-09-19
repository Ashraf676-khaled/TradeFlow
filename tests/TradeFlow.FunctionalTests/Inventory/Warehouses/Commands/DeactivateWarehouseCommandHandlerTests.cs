namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Commands;

using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Commands.DeactivateWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class DeactivateWarehouseCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithActiveEmptyWarehouse_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var handler = new DeactivateWarehouseCommandHandler(context);
    var result = await handler.Handle(new DeactivateWarehouseCommand(created.Value), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(context.Warehouses.Single().IsActive);
  }

  [Fact]
  public async Task Handle_WithAlreadyInactiveWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var handler = new DeactivateWarehouseCommandHandler(context);
    await handler.Handle(new DeactivateWarehouseCommand(created.Value), CancellationToken.None);
    var result = await handler.Handle(new DeactivateWarehouseCommand(created.Value), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.AlreadyInactive, result.TopError);
  }

  [Fact]
  public async Task Handle_WithNonExistentWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new DeactivateWarehouseCommandHandler(context);

    var result = await handler.Handle(new DeactivateWarehouseCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotFound, result.TopError);
  }

  [Fact]
  public async Task Handle_WithWarehouseHavingStock_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var tenantId = new TenantId(_currentUser.TenantId!.Value);
    var stockItemResult = StockItem.Create(tenantId, new WarehouseId(created.Value), ProductId.New());
    stockItemResult.Value.ReceiveStock(TradeFlow.Domain.Common.ValueObjects.Quantity.Create(10).Value);
    context.StockItems.Add(stockItemResult.Value);
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new DeactivateWarehouseCommandHandler(context);
    var result = await handler.Handle(new DeactivateWarehouseCommand(created.Value), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.HasStockOrActiveOrders, result.TopError);
  }
}
