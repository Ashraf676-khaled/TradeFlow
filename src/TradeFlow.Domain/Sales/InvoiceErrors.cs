namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Results;

public static class InvoiceErrors
{
  public static readonly Error InvalidTotalAmount = Error.Validation(
      "Invoice.InvalidTotalAmount", "Total amount must be greater than zero.");

  public static readonly Error InvalidPaymentAmount = Error.Validation(
      "Invoice.InvalidPaymentAmount", "Payment amount must be greater than zero.");

  public static readonly Error PaymentExceedsOutstandingBalance = Error.Conflict(
      "Invoice.PaymentExceedsOutstandingBalance", "Payment cannot exceed the outstanding balance.");

  public static readonly Error AlreadyCompleted = Error.Conflict(
      "Invoice.AlreadyCompleted", "Completed invoice cannot receive another payment.");

  public static readonly Error InvoiceCancelled = Error.Conflict(
      "Invoice.InvoiceCancelled", "Cannot register a payment on a cancelled invoice.");

  public static readonly Error AlreadyCancelled = Error.Conflict(
      "Invoice.AlreadyCancelled", "Invoice is already cancelled.");

  public static readonly Error CannotCancelInvoiceWithPayments = Error.Conflict(
      "Invoice.CannotCancelInvoiceWithPayments", "Cannot cancel an invoice that already has registered payments.");

  public static readonly Error NotFound = Error.NotFound(
      "Invoice.NotFound", "Invoice not found.");
}
