using Domain.Abstractions;
using MediatR;

namespace Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
  private readonly IMediator _mediator;

  public DomainEventDispatcher(IMediator mediator)
  {
    _mediator = mediator;
  }

  public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
  {
    foreach (var domainEvent in events)
    {
      await _mediator.Publish(domainEvent, ct);
    }
  }
}
