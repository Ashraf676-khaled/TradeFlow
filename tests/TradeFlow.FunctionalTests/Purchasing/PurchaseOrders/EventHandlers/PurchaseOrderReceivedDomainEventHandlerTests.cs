namespace TradeFlow.Application.UnitTests.Purchasing.PurchaseOrders.EventHandlers;

using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Purchasing.PurchaseOrders.EventHandlers;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Purchasing.Events;
using Xunit;

public class PurchaseOrderReceivedDomainEventHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_ShouldCreateStockItemAndIncreaseQuantity()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var domainEvent = new PurchaseOrderReceivedDomainEvent(
        Guid.NewGuid(),
        _currentUser.TenantId!.Value,
        warehouseId,
        [new PurchaseOrderItemReceivedSnapshot(productId, 15, 60)]);   // (ProductId, ReceivedQuantity, UnitCost)

    var handler = new PurchaseOrderReceivedDomainEventHandler(context);
    await handler.Handle(domainEvent, CancellationToken.None);

    Assert.Single(context.StockItems);
    Assert.Equal(15, context.StockItems.First().AvailableQuantity.Value);
  }

  [Fact]
  public async Task Handle_WhenStockItemAlreadyExists_ShouldAccumulate()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var handler = new PurchaseOrderReceivedDomainEventHandler(context);

    await handler.Handle(new PurchaseOrderReceivedDomainEvent(
        Guid.NewGuid(), _currentUser.TenantId!.Value, warehouseId,
        [new PurchaseOrderItemReceivedSnapshot(productId, 10, 60)]), CancellationToken.None);

    await handler.Handle(new PurchaseOrderReceivedDomainEvent(
        Guid.NewGuid(), _currentUser.TenantId!.Value, warehouseId,
        [new PurchaseOrderItemReceivedSnapshot(productId, 5, 60)]), CancellationToken.None);

    Assert.Single(context.StockItems);
    Assert.Equal(15, context.StockItems.First().AvailableQuantity.Value);
  }
}
