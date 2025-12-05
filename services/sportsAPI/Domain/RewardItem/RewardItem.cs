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

    public static RewardItem Create(OrganizationId orgId, long voteValue, string qrCode, string redemptionId)
    {
      if (voteValue <= 0)
        throw new DomainExceptions("RewardItem must have a positive VoteValue.");

      if (string.IsNullOrWhiteSpace(qrCode))
        throw new DomainExceptions("QR Code cannot be empty.");

      if (string.IsNullOrWhiteSpace(redemptionId))
        throw new DomainExceptions("RedemptionId cannot be empty.");

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
        throw new DomainExceptions("Reward already redeemed.");

      if (string.IsNullOrWhiteSpace(redemptionId))
        throw new DomainExceptions("RedemptionId required.");

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
        throw new DomainExceptions("Reward already redeemed.");

      RedeemedBy = userId;
      RedeemedAt = DateTime.UtcNow;
    }
  }
}
