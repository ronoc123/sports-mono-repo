namespace Domain.Purchase
{
    public sealed class ProcessedWebhookEvent : Aggregate<Guid>
    {
        public string StripeEventId { get; private set; } = default!;
        public string EventType { get; private set; } = default!;
        public DateTime ProcessedAt { get; private set; }

        private ProcessedWebhookEvent() { }

        public static ProcessedWebhookEvent Record(string stripeEventId, string eventType)
            => new()
            {
                Id = Guid.NewGuid(),
                StripeEventId = stripeEventId,
                EventType = eventType,
                ProcessedAt = DateTime.UtcNow
            };
    }
}
