namespace TradeFlow.Domain.Common.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
  protected abstract IEnumerable<object?> GetEqualityComponents();

  public bool Equals(ValueObject? other)
  {
    if (other is null || GetType() != other.GetType()) return false;
    return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
  }

  public override bool Equals(object? obj) => Equals(obj as ValueObject);

  public override int GetHashCode() =>
      GetEqualityComponents().Aggregate(1, (hash, c) => hash * 23 + (c?.GetHashCode() ?? 0));

  public static bool operator ==(ValueObject? a, ValueObject? b) =>
      a is null && b is null || (a is not null && a.Equals(b));

  public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
