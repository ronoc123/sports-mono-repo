namespace Domain.VoteAccount
{
  using Domain.Rewards;
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

        private readonly List<VoteTransaction> _transactions = new();
        public IReadOnlyList<VoteTransaction> Transactions => _transactions;

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

        public void ApplyReward(RedemptionToken token)
        {
          if (token.Amount <= 0) throw new DomainExceptions("Invalid reward amount.");
          Balance += token.Amount;

          _transactions.Add(
              VoteTransaction.ForRewardCredit(
                  token.OrgId,
                  token.RedeemingUser,
                  token.Amount,
                  token.RewardItemId.ToString()
              ));
          }


        public SpendToken AuthorizeSpend(PlayerOptionId optionId, long amount, string spendId)
        {
          ArgumentNullException.ThrowIfNull(optionId);
          ArgumentException.ThrowIfNullOrWhiteSpace(spendId);
          if (amount <= 0) throw new DomainExceptions("Spend amount must be positive.");


          if (Balance < amount)
            throw new DomainExceptions("Insufficient balance.");


          return new SpendToken(Id, OrgId, optionId, amount, spendId);
        }

        public void ApplySpend(SpendToken token)
        {

          if (Balance < token.Amount)
            throw new DomainExceptions("Insufficient balance at finalization.");

          Balance -= token.Amount;

          _transactions.Add(VoteTransaction.ForVoteSpend(OrgId, UserId, token.PlayerOptionId, token.Amount,token.SpendId));
         }

  }
}
