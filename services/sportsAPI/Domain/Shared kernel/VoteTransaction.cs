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
  }


}
