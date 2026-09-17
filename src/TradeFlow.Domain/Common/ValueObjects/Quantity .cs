namespace TradeFlow.Domain.Common.ValueObjects;

using TradeFlow.Domain.Common.Abstractions;
using TradeFlow.Domain.Common.Results;

public sealed class Quantity : ValueObject
{
  public int Value { get; }

  private Quantity(int value)
  {
    Value = value;
  }

  public static Result<Quantity> Create(int value)
  {
    if (value <= 0)
      return QuantityErrors.MustBePositive;

    return new Quantity(value);
  }

  public static Quantity Zero() => new(0);

  public Result<Quantity> Add(Quantity other)
  {
    return new Quantity(Value + other.Value);
  }

  public Result<Quantity> Subtract(Quantity other)
  {
    var result = Value - other.Value;

    if (result < 0)
      return QuantityErrors.ResultCannotBeNegative;

    return new Quantity(result);
  }

  public bool IsGreaterThan(Quantity other) => Value > other.Value;

  public bool IsGreaterThanOrEqual(Quantity other) => Value >= other.Value;

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Value;
  }

  public override string ToString() => Value.ToString();
}

public static class QuantityErrors
{
  public static readonly Error MustBePositive = Error.Validation(
      "Quantity.MustBePositive", "Quantity must be greater than zero.");

  public static readonly Error ResultCannotBeNegative = Error.Validation(
      "Quantity.ResultCannotBeNegative", "Resulting quantity cannot be negative.");
}
