using MediatR;

namespace SportifyCore.Domain;

public record DomainEvent : IDomainEvent, INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
