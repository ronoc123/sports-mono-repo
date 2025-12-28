namespace Domain.Rewards
{
    using BuildingBlocks.Exceptions;
    using Domain.Abstractions;
    using Domain.ValueObjects.ConcreteTypes;

    /// <summary>
    /// A gift-card-like reward unit issued by an Organization.
    /// Each RewardItem is redeemable ONCE by a User to increase their VoteAccount balance.
    /// </summary>
    public sealed class RewardItem : Aggregate<RewardItemId>
    {
        public OrganizationId OrganizationId { get; private set; }
        public long VoteValue { get; private set; }
        public string QrCode { get; private set; } = default!;
        public UserId? RedeemedBy { get; private set; }
        public DateTime? RedeemedAt { get; private set; }
        public bool IsRedeemed => RedeemedBy is not null;

        internal RewardItem() { }

        public static RewardItem Create(OrganizationId orgId, long voteValue, string qrCode = null, string redemptionId = null)
        {
            if (voteValue <= 0)
                throw new DomainException("RewardItem must have a positive VoteValue.");

            var item = new RewardItem
            {
                Id = RewardItemId.Of(Guid.NewGuid()),
                OrganizationId = orgId,
                VoteValue = voteValue,
                QrCode = qrCode,
            };

            return item;
        }

        internal RedemptionToken GenerateRedemption(UserId userId, string redemptionId)
        {
            if (IsRedeemed)
                throw new DomainException("Reward already redeemed.");

            if (string.IsNullOrWhiteSpace(redemptionId))
                throw new DomainException("RedemptionId required.");

            return new RedemptionToken(
                RewardItemId: Id,
                OrgId: OrganizationId,
                RedeemingUser: userId,
                Amount: VoteValue,
                RedemptionId: redemptionId);
        }

        internal void MarkRedeemed(UserId userId)
        {
            if (IsRedeemed)
                throw new DomainException("Reward already redeemed.");

            RedeemedBy = userId;
            RedeemedAt = DateTime.UtcNow;
        }
    }
}
