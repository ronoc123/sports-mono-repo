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
        public Guid Id { get; private set; } 
        public OrganizationId OrgId { get; private set; } = default!;
        public UserId UserId { get; private set; } = default!;
        public PlayerOptionId PlayerOptionId { get; private set; } = default!;
        public long Amount { get; private set; }
        public string Reason { get; private set; } = "vote_spend";
        public string SpendId { get; private set; } = default!;
        public DateTimeOffset CreatedAt { get; private set; }

        private VoteTransaction() { } // EF

        public static VoteTransaction ForVoteSpend(
            OrganizationId orgId,
            UserId userId,
            PlayerOptionId optionId,
            long amount,
            string spendId,
            DateTimeOffset? at = null)
        {
          if (amount <= 0) throw new DomainExceptions("Vote amount must be positive.");
          ArgumentException.ThrowIfNullOrWhiteSpace(spendId);

          return new VoteTransaction
          {
            OrgId = orgId,
            UserId = userId,
            PlayerOptionId = optionId,
            Amount = amount,
            Reason = "vote_spend",
            SpendId = spendId,
            CreatedAt = at ?? DateTimeOffset.UtcNow
          };
        }
      }

      // Token produced by VoteAccount (another AR)
      public sealed record SpendToken(
          VoteAccountId AccountId,
          OrganizationId OrgId,
          PlayerOptionId PlayerOptionId,
          long Amount,
          string SpendId);
}
