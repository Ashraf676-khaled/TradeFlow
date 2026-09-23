namespace TradeFlow.Application.UnitTests.Sales.Invoices.Commands;

using TradeFlow.Application.Customers.Commands.CreateCustomer;
using TradeFlow.Application.Inventory.Products.Commands.CreateProduct;
using TradeFlow.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using TradeFlow.Application.Sales.Invoices.Commands.CreateInvoiceFromOrder;
using TradeFlow.Application.Sales.Invoices.Commands.RegisterPayment;
using TradeFlow.Application.Sales.SalesOrders.Commands.CreateSalesOrder;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Sales;
using Xunit;

public class RegisterPaymentCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid(), UserId = Guid.NewGuid() };

  private async Task<Guid> CreateInvoiceAsync(TradeFlow.Infrastructure.Data.AppDbContext context)
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

    return (await new CreateInvoiceFromOrderCommandHandler(context, _currentUser)
        .Handle(new CreateInvoiceFromOrderCommand(orderId), CancellationToken.None)).Value;
  }

  [Fact]
  public async Task Handle_PartialPayment_ShouldSetStatusToPartiallyPaid()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var invoiceId = await CreateInvoiceAsync(context); // Total = 500

    var handler = new RegisterPaymentCommandHandler(context);
    var result = await handler.Handle(new RegisterPaymentCommand(invoiceId, 200), CancellationToken.None);

    Assert.True(result.IsSuccess);

    var invoice = context.Invoices.First(i => i.Id.Value == invoiceId);
    Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
  }

  [Fact]
  public async Task Handle_FullPayment_ShouldSetStatusToCompleted()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var invoiceId = await CreateInvoiceAsync(context); // Total = 500

    var handler = new RegisterPaymentCommandHandler(context);
    var result = await handler.Handle(new RegisterPaymentCommand(invoiceId, 500), CancellationToken.None);

    Assert.True(result.IsSuccess);

    var invoice = context.Invoices.First(i => i.Id.Value == invoiceId);
    Assert.Equal(InvoiceStatus.Completed, invoice.Status);
  }

  [Fact]
  public async Task Handle_PaymentExceedingBalance_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var invoiceId = await CreateInvoiceAsync(context); // Total = 500

    var handler = new RegisterPaymentCommandHandler(context);
    var result = await handler.Handle(new RegisterPaymentCommand(invoiceId, 600), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.PaymentExceedsOutstandingBalance, result.TopError);
  }
}
