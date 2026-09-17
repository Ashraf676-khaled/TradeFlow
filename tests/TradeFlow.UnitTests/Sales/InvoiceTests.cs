namespace TradeFlow.Domain.Tests.Sales;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Sales;
using Xunit;

public class InvoiceTests
{
  private static readonly TenantId TestTenant = TenantId.New();
  private static readonly SalesOrderId TestOrder = SalesOrderId.New();
  private static readonly CustomerId TestCustomer = CustomerId.New();

  private static Invoice CreateValidInvoice(decimal totalAmount = 1000)
  {
    var invoiceNumber = DocumentNumber.Generate("INV", 1).Value;
    var total = Money.EGP(totalAmount).Value;
    var dueDate = DateTimeOffset.UtcNow.AddDays(30);

    return Invoice.Create(TestTenant, TestOrder, TestCustomer, invoiceNumber, total, dueDate).Value;
  }

  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var invoiceNumber = DocumentNumber.Generate("INV", 1).Value;
    var total = Money.EGP(1000).Value;
    var dueDate = DateTimeOffset.UtcNow.AddDays(30);

    var result = Invoice.Create(TestTenant, TestOrder, TestCustomer, invoiceNumber, total, dueDate);

    Assert.True(result.IsSuccess);
    Assert.Equal(InvoiceStatus.Unpaid, result.Value.Status);
    Assert.Empty(result.Value.Payments);
  }

  [Fact]
  public void Create_WithZeroTotalAmount_ShouldFail()
  {
    var invoiceNumber = DocumentNumber.Generate("INV", 1).Value;
    var dueDate = DateTimeOffset.UtcNow.AddDays(30);

    var result = Invoice.Create(TestTenant, TestOrder, TestCustomer, invoiceNumber, Money.Zero(), dueDate);

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.InvalidTotalAmount, result.TopError);
  }

  [Fact]
  public void CalculateOutstandingBalance_WithNoPayments_ShouldEqualTotal()
  {
    var invoice = CreateValidInvoice(1000);

    var result = invoice.CalculateOutstandingBalance();

    Assert.True(result.IsSuccess);
    Assert.Equal(1000, result.Value.Amount);
  }

  [Fact]
  public void RegisterPayment_PartialAmount_ShouldSetStatusToPartiallyPaid()
  {
    var invoice = CreateValidInvoice(1000);

    var result = invoice.RegisterPayment(Money.EGP(400).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
    Assert.Equal(600, invoice.CalculateOutstandingBalance().Value.Amount);
  }

  [Fact]
  public void RegisterPayment_FullAmount_ShouldSetStatusToCompleted()
  {
    var invoice = CreateValidInvoice(1000);

    var result = invoice.RegisterPayment(Money.EGP(1000).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(InvoiceStatus.Completed, invoice.Status);
    Assert.Equal(0, invoice.CalculateOutstandingBalance().Value.Amount);
  }

  [Fact]
  public void RegisterPayment_MultiplePartialPayments_ShouldAccumulateCorrectly()
  {
    var invoice = CreateValidInvoice(1000);

    invoice.RegisterPayment(Money.EGP(300).Value);
    invoice.RegisterPayment(Money.EGP(300).Value);
    var result = invoice.RegisterPayment(Money.EGP(400).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(InvoiceStatus.Completed, invoice.Status);
    Assert.Equal(3, invoice.Payments.Count);
  }

  [Fact]
  public void RegisterPayment_ExceedingOutstandingBalance_ShouldFail()
  {
    var invoice = CreateValidInvoice(1000);
    invoice.RegisterPayment(Money.EGP(800).Value);

    var result = invoice.RegisterPayment(Money.EGP(300).Value); // الباقي بس 200

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.PaymentExceedsOutstandingBalance, result.TopError);
  }

  [Fact]
  public void RegisterPayment_WithZeroAmount_ShouldFail()
  {
    var invoice = CreateValidInvoice(1000);

    var result = invoice.RegisterPayment(Money.Zero());

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.InvalidPaymentAmount, result.TopError);
  }

  [Fact]
  public void RegisterPayment_OnCompletedInvoice_ShouldFail()
  {
    var invoice = CreateValidInvoice(1000);
    invoice.RegisterPayment(Money.EGP(1000).Value);

    var result = invoice.RegisterPayment(Money.EGP(100).Value);

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.AlreadyCompleted, result.TopError);
  }

  [Fact]
  public void RegisterPayment_OnCancelledInvoice_ShouldFail()
  {
    var invoice = CreateValidInvoice(1000);
    invoice.Cancel();

    var result = invoice.RegisterPayment(Money.EGP(100).Value);

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.InvoiceCancelled, result.TopError);
  }

  [Fact]
  public void Cancel_WithNoPayments_ShouldSucceed()
  {
    var invoice = CreateValidInvoice();

    var result = invoice.Cancel();

    Assert.True(result.IsSuccess);
    Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
  }

  [Fact]
  public void Cancel_WithExistingPayments_ShouldFail()
  {
    var invoice = CreateValidInvoice(1000);
    invoice.RegisterPayment(Money.EGP(200).Value);

    var result = invoice.Cancel();

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.CannotCancelInvoiceWithPayments, result.TopError);
  }

  [Fact]
  public void Cancel_WhenAlreadyCancelled_ShouldFail()
  {
    var invoice = CreateValidInvoice();
    invoice.Cancel();

    var result = invoice.Cancel();

    Assert.True(result.IsError);
    Assert.Equal(InvoiceErrors.AlreadyCancelled, result.TopError);
  }
}
