namespace TradeFlow.Application.UnitTests.Sales.Invoices;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales;
using Xunit;

public sealed class InvoicePaymentPersistenceTests
{
  [Fact]
  public async Task Save_InvoiceWithPayment_DoesNotConfuseOwnedMoneyForeignKeys()
  {
    var currentUser = new FakeCurrentUserService
    {
      TenantId = Guid.NewGuid(),
      UserId = Guid.NewGuid()
    };
    await using var context = TestDbContextFactory.Create(currentUser);

    var invoiceNumber = DocumentNumber.Generate("INV", 1);
    Assert.True(invoiceNumber.IsSuccess);
    var total = Money.EGP(100);
    Assert.True(total.IsSuccess);
    var invoiceResult = Invoice.Create(
      new TenantId(currentUser.TenantId.Value),
      SalesOrderId.New(),
      CustomerId.New(),
      invoiceNumber.Value,
      total.Value,
      DateTimeOffset.UtcNow.AddDays(30));
    Assert.True(invoiceResult.IsSuccess);

    var paymentResult = invoiceResult.Value.RegisterPayment(total.Value);
    Assert.True(paymentResult.IsSuccess);

    context.Invoices.Add(invoiceResult.Value);
    await context.SaveChangesAsync();

    context.ChangeTracker.Clear();
    var savedInvoice = await context.Invoices
      .Include(invoice => invoice.Payments)
      .SingleAsync(invoice => invoice.Id == invoiceResult.Value.Id);
    Assert.NotNull(savedInvoice);
    Assert.Single(savedInvoice.Payments);
    Assert.Equal(100, savedInvoice.Payments.Single().Amount.Amount);
  }
}
