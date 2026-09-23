namespace TradeFlow.Application.UnitTests.Sales.Invoices.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Sales;
using Xunit;

public class CreateInvoiceFromOrderCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid(), UserId = Guid.NewGuid() };

  private async Task<Guid> CreateConfirmedOrderAsync(TradeFlow.Infrastructure.Data.AppDbContext context)
  {
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
    await context.SaveChangesAsync(CancellationToken.None);

    return orderId;
  }

  [Fact]
  public async Task Handle_ForConfirmedOrder_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var orderId = await CreateConfirmedOrderAsync(context);

    var handler = new CreateInvoiceFromOrderCommandHandler(context, _currentUser);
    var result = await handler.Handle(new CreateInvoiceFromOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.Invoices);
  }

  [Fact]
  public async Task Handle_ForDraftOrder_ShouldFail()
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

    var handler = new CreateInvoiceFromOrderCommandHandler(context, _currentUser);
    var result = await handler.Handle(new CreateInvoiceFromOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SalesOrderErrors.NotConfirmed, result.TopError);
  }

  [Fact]
  public async Task Handle_WhenAlreadyInvoiced_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var orderId = await CreateConfirmedOrderAsync(context);

    var handler = new CreateInvoiceFromOrderCommandHandler(context, _currentUser);
    await handler.Handle(new CreateInvoiceFromOrderCommand(orderId), CancellationToken.None);
    var result = await handler.Handle(new CreateInvoiceFromOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.AlreadyInvoiced, result.TopError);
  }
}
