using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared_kernel
{
  // Domain-level ledger row (no Infra dependency)
  public sealed class VoteTransaction
  {
    public long Id { get; private set; }

    public OrganizationId OrgId { get; private set; }
    public UserId UserId { get; private set; }
    public long Amount { get; private set; }
    public string Reason { get; private set; } = default!;
    public string? RefId { get; private set; }
    public PlayerOptionId? PlayerOptionId { get; private set; }
    public string? SpendId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private VoteTransaction() { } // EF

    private VoteTransaction(
        OrganizationId orgId,
        UserId userId,
        long amount,
        string reason,
        string refId,
        PlayerOptionId? playerOptionId,
        string? spendId)
    {
      OrgId = orgId;
      UserId = userId;
      Amount = amount;
      Reason = reason;
      RefId = refId;
      PlayerOptionId = playerOptionId;
      SpendId = spendId;
      CreatedAt = DateTimeOffset.UtcNow;
    }

    public static VoteTransaction ForVoteSpend(
        OrganizationId orgId,
        UserId userId,
        PlayerOptionId optionId,
        long amount,
        string spendId)
        => new VoteTransaction(orgId, userId, -amount, "vote_spend", null, optionId, spendId);


    public static VoteTransaction ForRewardCredit(
        OrganizationId orgId,
        UserId userId,
        long amount,
        string rewardItemId)
        => new VoteTransaction(orgId, userId, amount, "reward_redeem", rewardItemId, null, null);

    public static VoteTransaction ForAdjust(
        OrganizationId orgId,
        UserId userId,
        long delta)
        => new VoteTransaction(orgId, userId, delta, "adjust", null, null, null);

    // ── Fan Economy factory methods ──────────────────────────────────────────

    /// <summary>Fan spends points to open a card pack.</summary>
    public static VoteTransaction ForPackPurchase(
        OrganizationId orgId, UserId userId, long amount, string packId)
        => new VoteTransaction(orgId, userId, -amount, "pack_purchase", packId, null, null);

    /// <summary>Points locked when a fan places a bid (balance debited).</summary>
    public static VoteTransaction ForBidEscrow(
        OrganizationId orgId, UserId userId, long amount, string listingId)
        => new VoteTransaction(orgId, userId, -amount, "bid_escrow", listingId, null, null);

    /// <summary>Escrowed points returned when a fan is outbid.</summary>
    public static VoteTransaction ForBidRelease(
        OrganizationId orgId, UserId userId, long amount, string listingId)
        => new VoteTransaction(orgId, userId, amount, "bid_release", listingId, null, null);

    /// <summary>Seller receives points when an auction settles or buy now occurs.</summary>
    public static VoteTransaction ForAuctionSaleCredit(
        OrganizationId orgId, UserId userId, long amount, string listingId)
        => new VoteTransaction(orgId, userId, amount, "auction_sale", listingId, null, null);

    /// <summary>Buyer spends points at the buy now price, ending the auction early.</summary>
    public static VoteTransaction ForBuyNowDebit(
        OrganizationId orgId, UserId userId, long amount, string listingId)
        => new VoteTransaction(orgId, userId, -amount, "buy_now", listingId, null, null);

    /// <summary>Points wagered at the start of an H2H match (deducted from balance).</summary>
    public static VoteTransaction ForH2HWager(
        OrganizationId orgId, UserId userId, long amount, string matchId)
        => new VoteTransaction(orgId, userId, -amount, "h2h_wager", matchId, null, null);

    /// <summary>Winner receives wager × 2 when an H2H match resolves in their favour.</summary>
    public static VoteTransaction ForH2HWin(
        OrganizationId orgId, UserId userId, long amount, string matchId)
        => new VoteTransaction(orgId, userId, amount, "h2h_win", matchId, null, null);
  }


}
