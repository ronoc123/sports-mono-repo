namespace Domain.VoteAccount
{
  using Domain.Shared_kernel;
  using Domain.ValueObjects.ConcreteTypes;

  /// <summary>
  /// Per-organization wallet for a user. AR key = (OrgId, UserId).
  /// Only this AR mutates vote balances.
  /// </summary>
  public sealed class VoteAccount : Aggregate<VoteAccountId>
  {
        internal VoteAccount() { } // EF

        public OrganizationId OrgId { get; private set; } = default!;
        public UserId UserId { get; private set; } = default!;

        public long Balance { get; private set; }
        public long Version { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // Idempotency for spends authorized by this account
        private readonly HashSet<string> _appliedSpendIds = new();

        private VoteAccount(VoteAccountId id, long initialBalance)
        {
          Id = id;
          OrgId = id.OrgId;
          UserId = id.UserId;
          Balance = initialBalance;
          Version = 0;
          CreatedAt = UpdatedAt = DateTime.UtcNow;
        }

        public static VoteAccount Create(OrganizationId orgId, UserId userId, long initialBalance = 0)
        {
          if (initialBalance < 0) throw new DomainExceptions("Initial balance cannot be negative.");
          return new VoteAccount(VoteAccountId.Of(orgId, userId), initialBalance);
        }

        public void Credit(long amount, string reason, long? refId = null)
        {
          if (amount <= 0) throw new DomainExceptions("Credit amount must be positive.");
          Balance += amount;
          touch();
          // AddEvent(new VotesCredited(Id, amount, reason, refId, DateTimeOffset.UtcNow));
        }

        public SpendToken AuthorizeSpend(PlayerOptionId optionId, long amount, string spendId)
        {
          ArgumentNullException.ThrowIfNull(optionId);
          ArgumentException.ThrowIfNullOrWhiteSpace(spendId);
          if (amount <= 0) throw new DomainExceptions("Spend amount must be positive.");

          // If we already authorized this spendId, re-issue the token (no double-debit).
          if (_appliedSpendIds.Contains(spendId))
            return new SpendToken(Id, OrgId, optionId, amount, spendId);

          if (Balance < amount) throw new DomainExceptions("Insufficient balance.");

          Balance -= amount;
          _appliedSpendIds.Add(spendId);
          touch();
          // AddEvent(new VotesSpent(Id, optionId, amount, spendId, DateTimeOffset.UtcNow));

          return new SpendToken(Id, OrgId, optionId, amount, spendId);
        }

        public void Refund(long amount, string spendId, string reason = "refund")
        {
          ArgumentException.ThrowIfNullOrWhiteSpace(spendId);
          if (amount <= 0) throw new DomainExceptions("Refund amount must be positive.");

          // We don't require the spendId to exist here—ops can choose policy.
          Balance += amount;
          touch();
          // AddEvent(new VotesRefunded(Id, amount, spendId, reason, DateTimeOffset.UtcNow));
        }

        public void Adjust(long delta, string reason = "adjust")
        {
          var newBal = Balance + delta;
          if (newBal < 0) throw new DomainExceptions("Adjustment would make balance negative.");
          Balance = newBal;
          touch();
          // AddEvent(new VotesAdjusted(Id, delta, reason, DateTimeOffset.UtcNow));
        }

        private void touch()
        {
          Version++;
          UpdatedAt = DateTime.UtcNow;
        }
  }
}
