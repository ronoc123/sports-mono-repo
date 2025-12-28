namespace BuildingBlocks.Messageing.Events
{
    public record RewardEmailRequestedEvent : IntergrationEvent
    {

        public Guid RewardId { get; set; }

        public string Email { get; set; }

        public DateTime RedeemedAt { get; set; }

    };

}
