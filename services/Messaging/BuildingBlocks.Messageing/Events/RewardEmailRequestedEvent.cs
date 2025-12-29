namespace BuildingBlocks.Messageing.Events
{
    public record RewardEmailRequestedEvent : IntergrationEvent
    {
        public Guid RewardId { get; init; }
        public Guid UserId { get; init; }
        public Guid OrganizationId { get; init; }
        public string Email { get; init; }
        public string Title { get; init; }
        public string Message { get; init; }
        public DateTime RedeemedAt { get; init; }

    };

}
