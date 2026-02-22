using BuildingBlocks.Exceptions;

namespace Domain.Purchase
{
    public sealed class Purchase : Aggregate<Guid>
    {
        internal Purchase() { }

        public UserId UserId { get; private set; } = default!;
        public OrganizationId OrgId { get; private set; } = default!;

        public PurchaseItem Item { get; private set; } = default!;
        public Money Price { get; private set; } = default!;

        public PurchaseStatus Status { get; private set; }
        public FulfillmentStatus FulfillmentStatus { get; private set; }

        public PaymentProvider Provider { get; private set; }
        public ExternalPaymentId? ExternalPaymentId { get; private set; }
        public ExternalSessionId? ExternalSessionId { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }

        private Purchase(Guid id, UserId userId, OrganizationId orgId, PurchaseItem item, Money price)
        {
            Id = id;
            UserId = userId;
            OrgId = orgId;
            Item = item;
            Price = price;
            Status = PurchaseStatus.Pending;
            FulfillmentStatus = FulfillmentStatus.NotStarted;
            CreatedAt = DateTime.UtcNow;
        }

        public static Purchase Create(Guid id, UserId userId, OrganizationId orgId, PurchaseItem item, Money price)
        {
            return new Purchase(id, userId, orgId, item, price);
        }

        public void AttachStripeSession(ExternalSessionId sessionId)
        {
            ExternalSessionId = sessionId;
        }

        public void MarkPaid(ExternalPaymentId paymentIntentId)
        {
            if (Status != PurchaseStatus.Pending)
                throw new DomainException("Invalid state transition.");

            Status = PurchaseStatus.Paid;
            ExternalPaymentId = paymentIntentId;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            if (Status != PurchaseStatus.Pending)
                throw new DomainException("Invalid state transition.");

            Status = PurchaseStatus.Failed;
        }

        public void MarkFulfilled()
        {
            if (Status != PurchaseStatus.Paid)
                throw new DomainException("Cannot fulfill unpaid purchase.");

            FulfillmentStatus = FulfillmentStatus.Completed;
        }
    }
}
