using TradeFlow.Domain.Common.Events;
using MediatR;

namespace TradeFlow.Domain.Common.Abstractions;

public abstract class AggregateRoot : Entity
{
  private readonly List<DomainEvent> _domainEvents = [];

  public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  protected AggregateRoot() { }
  protected AggregateRoot(Guid id) : base(id) { }

  public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
  public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
  public void ClearDomainEvents() => _domainEvents.Clear();
}
