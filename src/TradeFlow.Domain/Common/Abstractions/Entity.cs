namespace TradeFlow.Domain.Common.Abstractions;

public abstract class Entity : IEquatable<Entity>
{
  public Guid Id { get; protected set; }

  protected Entity() { }

  protected Entity(Guid id)
  {
    Id = id == Guid.Empty ? Guid.NewGuid() : id;
  }

  public bool Equals(Entity? other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    if (GetType() != other.GetType()) return false;
    return Id == other.Id;
  }

  public override bool Equals(object? obj) => Equals(obj as Entity);

  public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();

  public static bool operator ==(Entity? a, Entity? b) =>
      a is null && b is null || (a is not null && a.Equals(b));

  public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
