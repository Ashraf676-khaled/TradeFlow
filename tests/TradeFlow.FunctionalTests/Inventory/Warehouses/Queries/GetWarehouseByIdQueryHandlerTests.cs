namespace TradeFlow.Application.UnitTests.Inventory.Warehouses.Queries;

using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Inventory.Warehouses.Queries.GetWarehouseById;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class GetWarehouseByIdQueryHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithExistingWarehouse_ShouldReturnDto()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var mapper = TestMapperFactory.Create();

    var createHandler = new CreateWarehouseCommandHandler(context, _currentUser);
    var created = await createHandler.Handle(new CreateWarehouseCommand("Main Warehouse", "Cairo"), CancellationToken.None);

    var handler = new GetWarehouseByIdQueryHandler(context, mapper);
    var result = await handler.Handle(new GetWarehouseByIdQuery(created.Value), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("Main Warehouse", result.Value.Name);
    Assert.Equal("Cairo", result.Value.Location);
    Assert.True(result.Value.IsActive);
  }

  [Fact]
  public async Task Handle_WithNonExistentWarehouse_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var mapper = TestMapperFactory.Create();
    var handler = new GetWarehouseByIdQueryHandler(context, mapper);

    var result = await handler.Handle(new GetWarehouseByIdQuery(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(WarehouseErrors.NotFound, result.TopError);
  }
}
