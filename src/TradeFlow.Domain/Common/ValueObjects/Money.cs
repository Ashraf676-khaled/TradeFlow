namespace TradeFlow.Domain.Common.ValueObjects;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class Money : ValueObject
{
  public decimal Amount { get; }
  public string Currency { get; }

  private Money(decimal amount, string currency)
  {
    Amount = amount;
    Currency = currency;
  }

  public static Result<Money> EGP(decimal amount)
  {
    if (amount < 0)
      return MoneyErrors.NegativeAmount;

    return new Money(amount, "EGP");
  }

  public static Money Zero() => new(0, "EGP");

  public Result<Money> Add(Money other)
  {
    if (Currency != other.Currency)
      return MoneyErrors.CurrencyMismatch;

    return new Money(Amount + other.Amount, Currency);
  }

  public Result<Money> Subtract(Money other)
  {
    if (Currency != other.Currency)
      return MoneyErrors.CurrencyMismatch;

    var result = Amount - other.Amount;
    if (result < 0)
      return MoneyErrors.NegativeResult;

    return new Money(result, Currency);
  }

  public Result<Money> Multiply(int factor)
  {
    if (factor < 0)
      return MoneyErrors.NegativeFactor;

    return new Money(Amount * factor, Currency);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Amount;
    yield return Currency;
  }

  public override string ToString() => $"{Amount:N2} {Currency}";
}

public static class MoneyErrors
{
  public static readonly Error NegativeAmount = Error.Validation(
      "Money.NegativeAmount", "Amount cannot be negative.");

  public static readonly Error NegativeResult = Error.Validation(
      "Money.NegativeResult", "Resulting amount cannot be negative.");

  public static readonly Error NegativeFactor = Error.Validation(
      "Money.NegativeFactor", "Factor cannot be negative.");

  public static readonly Error CurrencyMismatch = Error.Validation(
      "Money.CurrencyMismatch", "Cannot perform operation between different currencies.");
}
