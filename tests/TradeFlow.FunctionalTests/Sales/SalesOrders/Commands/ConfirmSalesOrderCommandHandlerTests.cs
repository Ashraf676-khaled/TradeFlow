namespace TradeFlow.Application.UnitTests.Sales.SalesOrders.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.StockItems.Commands.ReceiveStock;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Sales.SalesOrders.Commands.ConfirmSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.Sales.SalesOrders.EventHandlers;
using TradeFlow.Application.Settings;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Sales;
using Xunit;

public class ConfirmSalesOrderCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid(), UserId = Guid.NewGuid() };

  private async Task<(Guid customerId, Guid warehouseId, Guid productId, Guid orderId)> SeedOrderAsync(
      TradeFlow.Infrastructure.Data.AppDbContext context, decimal creditLimit = 5000, int stockQuantity = 20)
  {
    var customerId = (await new CreateCustomerCommandHandler(context, _currentUser)
        .Handle(new CreateCustomerCommand("Ahmed Trading", "01012345678", creditLimit), CancellationToken.None)).Value;

    var warehouseId = (await new CreateWarehouseCommandHandler(context, _currentUser)
        .Handle(new CreateWarehouseCommand("Main WH", "Cairo"), CancellationToken.None)).Value;

    var productId = (await new CreateProductCommandHandler(context, _currentUser)
        .Handle(new CreateProductCommand("Door", "D-001", 100, 50, 5), CancellationToken.None)).Value;

    if (stockQuantity > 0)
    {
      await new ReceiveStockCommandHandler(context, _currentUser)
          .Handle(new ReceiveStockCommand(productId, warehouseId, stockQuantity), CancellationToken.None);
    }

    var orderId = (await new CreateSalesOrderCommandHandler(context, _currentUser)
        .Handle(new CreateSalesOrderCommand(customerId, warehouseId,
            [new Application.Sales.SalesOrders.Commands.CreateSalesOrder.SalesOrderItemInput(productId, 5, 100)]),
            CancellationToken.None)).Value;

    return (customerId, warehouseId, productId, orderId);
  }

  [Fact]
  public async Task Handle_WithSufficientCredit_ShouldSucceedAndIncreaseCustomerBalance()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (customerId, _, _, orderId) = await SeedOrderAsync(context, creditLimit: 5000);

    var handler = new ConfirmSalesOrderCommandHandler(context);
    var result = await handler.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsSuccess);

    var customer = context.Customers.First(c => c.Id.Value == customerId);
    Assert.Equal(500, customer.CurrentBalance.Amount); // 5 items * 100
  }

  [Fact]
  public async Task Handle_WhenExceedingCreditLimit_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    // حد الائتمان أقل من قيمة الأوردر (500) عمدًا
    var (_, _, _, orderId) = await SeedOrderAsync(context, creditLimit: 100);

    var handler = new ConfirmSalesOrderCommandHandler(context);
    var result = await handler.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.CreditLimitExceeded, result.TopError);
  }

  [Fact]
  public async Task Handle_WithEmptyOrder_ShouldFailBeforeConfirming()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new ConfirmSalesOrderCommandHandler(context);

    var result = await handler.Handle(new ConfirmSalesOrderCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(Domain.Sales.SalesOrderErrors.NotFound, result.TopError);
  }

  [Fact]
  public async Task ConfirmedOrder_ThenEventHandler_ShouldReserveStock()
  {
    // اختبار تكاملي: يحاكي الـFlow الكامل (Confirm ثم تشغيل الـEvent Handler يدويًا،
    // لأن الـInMemory DB مالهاش الـDispatchDomainEventsInterceptor الحقيقي شغال تلقائيًا)
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (_, warehouseId, productId, orderId) = await SeedOrderAsync(context, stockQuantity: 20);

    var order = context.SalesOrders.First(o => o.Id.Value == orderId);
    var confirmResult = order.Confirm();
    Assert.True(confirmResult.IsSuccess);

    var domainEvent = order.DomainEvents.OfType<Domain.Sales.Events.SalesOrderConfirmedDomainEvent>().Single();

    var eventHandler = new SalesOrderConfirmedDomainEventHandler(context);
    await eventHandler.Handle(domainEvent, CancellationToken.None);

    var stockItem = context.StockItems.First(s => s.WarehouseId.Value == warehouseId && s.ProductId.Value == productId);
    Assert.Equal(5, stockItem.ReservedQuantity.Value);
    Assert.Equal(15, stockItem.AvailableQuantity.Value); // 20 - 5 المحجوزة
  }

  [Fact]
  public async Task Handle_ShouldGenerateInvoiceAutomatically()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (_, _, _, orderId) = await SeedOrderAsync(context, creditLimit: 5000, stockQuantity: 20);

    var handler = new ConfirmSalesOrderCommandHandler(context);
    var result = await handler.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotEqual(Guid.Empty, result.Value.InvoiceId);

    var invoice = context.Invoices.Single();
    Assert.Equal(result.Value.InvoiceId, invoice.Id.Value);
    Assert.Equal(InvoiceStatus.Unpaid, invoice.Status); // credit sales enabled by default
    Assert.Equal(500, invoice.TotalAmount.Amount);     // 5 * 100
  }

  [Fact]
  public async Task Handle_WhenCreditSalesDisabled_ShouldIssueFullyPaidInvoice()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var (_, _, _, orderId) = await SeedOrderAsync(context, creditLimit: 5000, stockQuantity: 20);

    var tenantId = new TenantId(_currentUser.TenantId!.Value);
    context.SystemSettings.Add(
        Domain.Settings.SystemSetting.Create(tenantId, SystemSettingKeys.CreditSalesEnabled, "false").Value);
    await context.SaveChangesAsync(CancellationToken.None);

    var handler = new ConfirmSalesOrderCommandHandler(context);
    var result = await handler.Handle(new ConfirmSalesOrderCommand(orderId), CancellationToken.None);

    Assert.True(result.IsSuccess);

    var invoice = context.Invoices.Single();
    Assert.Equal(InvoiceStatus.Completed, invoice.Status); // cash/POS sale → paid immediately
    Assert.Single(invoice.Payments);
    Assert.Equal(500, invoice.TotalAmount.Amount);
  }
}
