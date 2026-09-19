namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Commands;

using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class CreateWarehouseCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateWarehouseCommandHandler(context, _currentUser);

    var command = new CreateWarehouseCommand("Main Warehouse", "Cairo");

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.Warehouses);
  }

  [Fact]
  public async Task Handle_WithDuplicateName_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateWarehouseCommandHandler(context, _currentUser);

    await handler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);
    var result = await handler.Handle(new CreateWarehouseCommand("Main Warehouse", "Alexandria"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NameAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_WithoutTenant_ShouldFail()
  {
    var noTenantUser = new FakeCurrentUserService { TenantId = null };
    await using var context = TestDbContextFactory.Create(noTenantUser);
    var handler = new CreateWarehouseCommandHandler(context, noTenantUser);

    var result = await handler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    Assert.True(result.IsError);
  }

  [Fact]
  public async Task Handle_SameNameDifferentTenant_ShouldSucceed()
  {
    var tenantA = new FakeCurrentUserService { TenantId = Guid.NewGuid() };
    var tenantB = new FakeCurrentUserService { TenantId = Guid.NewGuid() };

    await using var contextA = TestDbContextFactory.Create(tenantA);
    var handlerA = new CreateWarehouseCommandHandler(contextA, tenantA);
    await handlerA.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    await using var contextB = TestDbContextFactory.Create(tenantB);
    var handlerB = new CreateWarehouseCommandHandler(contextB, tenantB);
    var result = await handlerB.Handle(new CreateWarehouseCommand("Main Warehouse", "Giza"), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }
}
