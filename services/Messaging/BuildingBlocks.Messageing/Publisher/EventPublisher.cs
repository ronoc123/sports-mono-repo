using MassTransit;


namespace BuildingBlocks.Messageing.Publisher
{
  internal sealed class EventPublisher : IEventPublisher
  {
    private readonly IPublishEndpoint _publishEndpoint;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
      _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
      return _publishEndpoint.Publish(@event, ct);
    }
  }
}
