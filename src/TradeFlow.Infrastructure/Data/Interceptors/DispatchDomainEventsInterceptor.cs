namespace TradeFlow.Infrastructure.Data.Interceptors;

using TradeFlow.Domain.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class DispatchDomainEventsInterceptor(IPublisher mediator) : SaveChangesInterceptor
{
  private readonly IPublisher _mediator = mediator;

  public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
      DbContextEventData eventData,
      InterceptionResult<int> result,
      CancellationToken cancellationToken = default)
  {
    var context = eventData.Context;
    if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

    var aggregates = context.ChangeTracker.Entries<AggregateRoot>()
        .Where(e => e.Entity.DomainEvents.Count > 0)
        .Select(e => e.Entity)
        .ToList();

    var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
    aggregates.ForEach(a => a.ClearDomainEvents());

    foreach (var domainEvent in domainEvents)
    {
      await _mediator.Publish(domainEvent, cancellationToken);
    }

    return await base.SavingChangesAsync(eventData, result, cancellationToken);
  }
}
