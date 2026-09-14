namespace TradeFlow.Domain.Common.ValueObjects;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class Discount : ValueObject
{
  public decimal Percentage { get; }

  private Discount(decimal percentage)
  {
    Percentage = percentage;
  }

  public static Discount None() => new(0);

  public static Result<Discount> Create(decimal percentage)
  {
    if (percentage < 0 || percentage > 100)
      return DiscountErrors.OutOfRange;

    return new Discount(percentage);
  }

  public Result<Money> ApplyTo(Money amount)
  {
    var discountedAmount = amount.Amount - (amount.Amount * Percentage / 100);

    return Money.EGP(discountedAmount);
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Percentage;
  }

  public override string ToString() => $"{Percentage}%";
}

public static class DiscountErrors
{
  public static readonly Error OutOfRange = Error.Validation(
      "Discount.OutOfRange", "Discount must be between 0 and 100.");
}
