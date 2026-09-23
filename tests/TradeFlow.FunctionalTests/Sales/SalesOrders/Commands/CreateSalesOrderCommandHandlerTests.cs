namespace TradeFlow.Application.UnitTests.Sales.SalesOrders.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using Xunit;

public class CreateSalesOrderCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid(), UserId = Guid.NewGuid() };

  private async Task<(Guid customerId, Guid warehouseId, Guid productId)> SeedAsync(
      TradeFlow.Infrastructure.Data.AppDbContext context)
  {
    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;

    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    return (customerId, warehouseId, productId);
  }

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (customerId, warehouseId, productId) = await SeedAsync(context);

    var handler = new CreateSalesOrderCommandHandler(context, _currentUser);
    var command = new CreateSalesOrderCommand(customerId, warehouseId, [new SalesOrderItemInput(productId, 5, 100)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.SalesOrders);
  }

  [Fact]
  public async Task Handle_ForInactiveCustomer_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (customerId, warehouseId, productId) = await SeedAsync(context);

    var customer = context.Customers.First(c => c.Id.Value == customerId);
    customer.Deactivate();
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new CreateSalesOrderCommandHandler(context, _currentUser);
    var command = new CreateSalesOrderCommand(customerId, warehouseId, [new SalesOrderItemInput(productId, 5, 100)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.InactiveCustomer, result.TopError);
  }

  [Fact]
  public async Task Handle_ForInactiveProduct_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (customerId, warehouseId, productId) = await SeedAsync(context);

    var product = context.Products.First(p => p.Id.Value == productId);
    product.Deactivate();
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new CreateSalesOrderCommandHandler(context, _currentUser);
    var command = new CreateSalesOrderCommand(customerId, warehouseId, [new SalesOrderItemInput(productId, 5, 100)]);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(ProductErrors.NotSellable, result.TopError);
  }
}
