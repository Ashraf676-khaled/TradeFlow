namespace TradeFlow.Application.UnitTests.Inventory.StockItems.Commands;

using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.UnitTests.Helpers;
using Xunit;

public class ReceiveStockCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_FirstTimeReceiving_ShouldCreateStockItem()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;

    var handler = new ReceiveStockCommandHandler(context, _currentUser);
    var result = await handler.Handle(new ReceiveStockCommand(productId, warehouseId, 20), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.StockItems);
    Assert.Equal(20, context.StockItems.First().AvailableQuantity.Value);
  }

  [Fact]
  public async Task Handle_ReceivingAgain_ShouldAccumulateQuantity()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;

    var handler = new ReceiveStockCommandHandler(context, _currentUser);
    await handler.Handle(new ReceiveStockCommand(productId, warehouseId, 20), CancellationToken.None);
    await handler.Handle(new ReceiveStockCommand(productId, warehouseId, 30), CancellationToken.None);

    Assert.Single(context.StockItems); // نفس الـStockItem مش عنصر جديد
    Assert.Equal(50, context.StockItems.First().AvailableQuantity.Value);
  }
}
