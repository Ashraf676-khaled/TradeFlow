namespace TradeFlow.Application.UnitTests.Sales.SalesOrders.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Sales.SalesOrders.Commands.CancelSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Sales;
using Xunit;

public class CancelSalesOrderCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid(), UserId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_ForDraftOrder_ShouldSucceedWithoutTouchingCustomerBalance()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;
    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var orderId = (await new CreateSalesOrderCommandHandler(context, _currentUser)
        .Handle(new CreateSalesOrderCommand(customerId, warehouseId,
            [new SalesOrderItemInput(productId, 5, 100)]), CancellationToken.None)).Value;

    var handler = new CancelSalesOrderCommandHandler(context);
    var result = await handler.Handle(new CancelSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsSuccess);

    var customer = context.Customers.First(c => c.Id.Value == customerId);
    Assert.Equal(0, customer.CurrentBalance.Amount); // مكانش Confirmed، فمفيش رصيد اتحرك أصلًا
  }

  [Fact]
  public async Task Handle_ForCompletedOrder_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", 5000), CancellationToken.None)).Value;
    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;
    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    var orderId = (await new CreateSalesOrderCommandHandler(context, _currentUser)
        .Handle(new CreateSalesOrderCommand(customerId, warehouseId,
            [new SalesOrderItemInput(productId, 5, 100)]), CancellationToken.None)).Value;

    var order = context.SalesOrders.First(o => o.Id.Value == orderId);
    order.Confirm();
    order.Complete();
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new CancelSalesOrderCommandHandler(context);
    var result = await handler.Handle(new CancelSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.CannotCancelCompletedOrder, result.TopError);
  }
}
