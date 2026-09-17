namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Payment : Entity
{
  public Money Amount { get; private set; } = null!;
  public DateTimeOffset PaidAt { get; private set; }

  private Payment() { } // EF Core

  private Payment(Guid id, Money amount) : base(id)
  {
    Amount = amount;
    PaidAt = DateTimeOffset.UtcNow;
  }

  internal static Result<Payment> Create(Money amount)
  {
    if (amount.Amount <= 0)
      return InvoiceErrors.InvalidPaymentAmount;

    return new Payment(Guid.CreateVersion7(), amount);
  }
}
