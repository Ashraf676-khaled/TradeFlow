namespace TradeFlow.Domain.Sales;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;

public sealed class Payment : Entity
{
  public PaymentAmount Amount { get; private set; } = null!;
  public DateTimeOffset PaidAt { get; private set; }

  private Payment() { } // EF Core

  private Payment(Guid id, Money amount) : base(id)
  {
    Amount = PaymentAmount.From(amount);
    PaidAt = DateTimeOffset.UtcNow;
  }

  internal static Result<Payment> Create(Money amount)
  {
    if (amount.Amount <= 0)
      return InvoiceErrors.InvalidPaymentAmount;

    return new Payment(Guid.CreateVersion7(), amount);
  }
}

public sealed class PaymentAmount : ValueObject
{
  public decimal Amount { get; }
  public string Currency { get; } = null!;

  private PaymentAmount() { }

  private PaymentAmount(decimal amount, string currency)
  {
    Amount = amount;
    Currency = currency;
  }

  internal static PaymentAmount From(Money money) => new(money.Amount, money.Currency);

  internal Result<Money> ToMoney()
  {
    if (Currency != "EGP")
      return MoneyErrors.CurrencyMismatch;
    return Money.EGP(Amount);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Amount;
    yield return Currency;
  }
}
