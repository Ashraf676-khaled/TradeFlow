namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Invoice : AggregateRoot, IAuditableEntity
{
  private readonly List<Payment> _payments = [];

  public new InvoiceId Id { get; private set; }
  public TenantId TenantId { get; private set; }
  public SalesOrderId SalesOrderId { get; private set; }
  public CustomerId CustomerId { get; private set; }
  public DocumentNumber InvoiceNumber { get; private set; } = null!;
  public DateTimeOffset IssuedAt { get; private set; }
  public DateTimeOffset DueDate { get; private set; }
  public Money TotalAmount { get; private set; } = null!;
  public InvoiceStatus Status { get; private set; }

  public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

  DateTimeOffset IAuditableEntity.CreatedAtUtc { get; set; }
  string? IAuditableEntity.CreatedBy { get; set; }
  DateTimeOffset IAuditableEntity.LastModifiedUtc { get; set; }
  string? IAuditableEntity.LastModifiedBy { get; set; }

  private Invoice() { } // EF Core

  private Invoice(
      InvoiceId id,
      TenantId tenantId,
      SalesOrderId salesOrderId,
      CustomerId customerId,
      DocumentNumber invoiceNumber,
      Money totalAmount,
      DateTimeOffset dueDate)
      : base(id.Value)
  {
    Id = id;
    TenantId = tenantId;
    SalesOrderId = salesOrderId;
    CustomerId = customerId;
    InvoiceNumber = invoiceNumber;
    TotalAmount = totalAmount;
    IssuedAt = DateTimeOffset.UtcNow;
    DueDate = dueDate;
    Status = InvoiceStatus.Unpaid;
  }

  public static Result<Invoice> Create(
      TenantId tenantId,
      SalesOrderId salesOrderId,
      CustomerId customerId,
      DocumentNumber invoiceNumber,
      Money totalAmount,
      DateTimeOffset dueDate)
  {
    if (totalAmount.Amount <= 0)
      return InvoiceErrors.InvalidTotalAmount;

    return new Invoice(InvoiceId.New(), tenantId, salesOrderId, customerId, invoiceNumber, totalAmount, dueDate);
  }

  public Result<Money> CalculatePaidAmount()
  {
    var total = Money.Zero();

    foreach (var payment in _payments)
    {
      var result = total.Add(payment.Amount);
      if (result.IsError)
        return result.Errors;

      total = result.Value;
    }

    return total;
  }

  public Result<Money> CalculateOutstandingBalance()
  {
    var paidResult = CalculatePaidAmount();
    if (paidResult.IsError)
      return paidResult.Errors;

    return TotalAmount.Subtract(paidResult.Value);
  }

  public Result<Success> RegisterPayment(Money amount)
  {
    if (Status == InvoiceStatus.Completed)
      return InvoiceErrors.AlreadyCompleted;

    if (Status == InvoiceStatus.Cancelled)
      return InvoiceErrors.InvoiceCancelled;

    var outstandingResult = CalculateOutstandingBalance();
    if (outstandingResult.IsError)
      return outstandingResult.Errors;

    if (amount.Amount > outstandingResult.Value.Amount)
      return InvoiceErrors.PaymentExceedsOutstandingBalance;

    var paymentResult = Payment.Create(amount);
    if (paymentResult.IsError)
      return paymentResult.Errors;

    _payments.Add(paymentResult.Value);

    var newOutstandingResult = CalculateOutstandingBalance();
    if (newOutstandingResult.IsError)
      return newOutstandingResult.Errors;

    Status = newOutstandingResult.Value.Amount == 0
        ? InvoiceStatus.Completed
        : InvoiceStatus.PartiallyPaid;

    return Result.Success;
  }

  public Result<Success> Cancel()
  {
    if (Status == InvoiceStatus.Cancelled)
      return InvoiceErrors.AlreadyCancelled;

    if (_payments.Count > 0)
      return InvoiceErrors.CannotCancelInvoiceWithPayments;

    Status = InvoiceStatus.Cancelled;
    return Result.Success;
  }
}
