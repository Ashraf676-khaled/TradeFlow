namespace TradeFlow.Application.UnitTests.Purchasing.PurchaseOrders.Commands;

using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Purchasing.PurchaseOrders.Commands.CreatePurchaseOrder;
using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Purchasing;
using Xunit;

public class CreatePurchaseOrderCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  private async Task<(Guid supplierId, Guid warehouseId, Guid productId)> SeedAsync(
      TradeFlow.Infrastructure.Data.AppDbContext context)
  {
    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("Al-Amal Trading", "01012345678"), CancellationToken.None)).Value;

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;

    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    return (supplierId, warehouseId, productId);
  }

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (supplierId, warehouseId, productId) = await SeedAsync(context);

    var handler = new CreatePurchaseOrderCommandHandler(context, _currentUser);
    var command = new CreatePurchaseOrderCommand(
        supplierId, warehouseId, [new PurchaseOrderItemInput(productId, 10, 40)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.PurchaseOrders);
  }

  [Fact]
  public async Task Handle_WithNonExistingSupplier_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (_, warehouseId, productId) = await SeedAsync(context);

    var handler = new CreatePurchaseOrderCommandHandler(context, _currentUser);
    var command = new CreatePurchaseOrderCommand(
        Guid.NewGuid(), warehouseId, [new PurchaseOrderItemInput(productId, 10, 40)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SupplierErrors.NotFound, result.TopError);
  }

  [Fact]
  public async Task Handle_WithNonExistingProduct_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (supplierId, warehouseId, _) = await SeedAsync(context);

    var handler = new CreatePurchaseOrderCommandHandler(context, _currentUser);
    var command = new CreatePurchaseOrderCommand(
        supplierId, warehouseId, [new PurchaseOrderItemInput(Guid.NewGuid(), 10, 40)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
  }
}
