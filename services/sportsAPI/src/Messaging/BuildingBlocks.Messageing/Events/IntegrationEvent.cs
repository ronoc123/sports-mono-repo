﻿
namespace BuildingBlocks.Messageing.Events
{
    public record IntergrationEvent
    {
        public Guid Id => Guid.NewGuid();

        public DateTime OccurredOn => DateTime.Now;

        public string EventType => GetType().AssemblyQualifiedName;
    }
}
