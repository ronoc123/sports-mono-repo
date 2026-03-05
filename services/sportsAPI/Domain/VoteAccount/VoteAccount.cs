namespace Domain.VoteAccount
{
    using BuildingBlocks.Exceptions;
    using Domain.Shared_kernel;
    using Domain.ValueObjects.ConcreteTypes;

    /// <summary>
    /// Per-league wallet for a user. AR key = (LeagueId, UserId).
    /// Only this AR mutates vote balances.
    /// </summary>
    public sealed class VoteAccount : Aggregate<VoteAccountId>
    {
        internal VoteAccount() { } // EF

        public LeagueId LeagueId { get; private set; } = default!;
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
            LeagueId = id.LeagueId;
            UserId = id.UserId;
            Balance = initialBalance;
            Version = 0;
            CreatedAt = UpdatedAt = DateTime.UtcNow;
        }

        public static VoteAccount Create(LeagueId leagueId, UserId userId, long initialBalance = 0)
        {
            if (initialBalance < 0) throw new DomainException("Initial balance cannot be negative.");
            return new VoteAccount(VoteAccountId.Of(leagueId, userId), initialBalance);
        }

        public void ApplyReward(RedemptionToken token)
        {
            if (token.Amount <= 0) throw new DomainException("Invalid reward amount.");
            Balance += token.Amount;

            _transactions.Add(
                VoteTransaction.ForRewardCredit(
                    LeagueId,
                    token.RedeemingUser,
                    token.Amount,
                    token.RewardItemId.ToString()
                ));
        }

        public SpendToken AuthorizeSpend(PlayerOptionId optionId, long amount, string spendId)
        {
            ArgumentNullException.ThrowIfNull(optionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(spendId);
            if (amount <= 0) throw new DomainException("Spend amount must be positive.");

            if (Balance < amount)
                throw new DomainException("Insufficient Funds.");

            return new SpendToken(Id, LeagueId, optionId, amount, spendId);
        }

        public void ApplySpend(SpendToken token)
        {
            if (Balance < token.Amount)
                throw new DomainException("Insufficient balance at finalization.");

            Balance -= token.Amount;

            _transactions.Add(VoteTransaction.ForVoteSpend(LeagueId, UserId, token.PlayerOptionId, token.Amount, token.SpendId));
        }

        // ── Fan Economy balance operations ───────────────────────────────────

        /// <summary>Deduct points when a fan purchases a card pack.</summary>
        public void DeductForPackPurchase(long amount, string packId)
        {
            if (amount <= 0) throw new DomainException("Pack purchase amount must be positive.");
            if (Balance < amount) throw new DomainException("Insufficient balance.");
            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForPackPurchase(LeagueId, UserId, amount, packId));
        }

        /// <summary>Deduct and escrow points when a fan places a bid.</summary>
        public void DeductForBidEscrow(long amount, string listingId)
        {
            if (amount <= 0) throw new DomainException("Bid amount must be positive.");
            if (Balance < amount) throw new DomainException("Insufficient balance.");
            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForBidEscrow(LeagueId, UserId, amount, listingId));
        }

        /// <summary>Restore escrowed points to the balance when a fan is outbid.</summary>
        public void CreditBidRelease(long amount, string listingId)
        {
            if (amount <= 0) throw new DomainException("Release amount must be positive.");
            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForBidRelease(LeagueId, UserId, amount, listingId));
        }

        /// <summary>Credit points to seller when auction settles or buy now is triggered.</summary>
        public void CreditAuctionSale(long amount, string listingId)
        {
            if (amount <= 0) throw new DomainException("Sale credit amount must be positive.");
            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForAuctionSaleCredit(LeagueId, UserId, amount, listingId));
        }

        /// <summary>Deduct points when buyer completes a buy now purchase.</summary>
        public void DeductForBuyNow(long amount, string listingId)
        {
            if (amount <= 0) throw new DomainException("Buy now amount must be positive.");
            if (Balance < amount) throw new DomainException("Insufficient balance.");
            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForBuyNowDebit(LeagueId, UserId, amount, listingId));
        }

        /// <summary>Deduct wager points at the start of an H2H match.</summary>
        public void DeductForH2HWager(long amount, string matchId)
        {
            if (amount <= 0) throw new DomainException("Wager amount must be positive.");
            if (Balance < amount) throw new DomainException("Insufficient balance.");
            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForH2HWager(LeagueId, UserId, amount, matchId));
        }

        /// <summary>Credit wager × 2 to the winner when an H2H match resolves.</summary>
        public void CreditH2HWinnings(long amount, string matchId)
        {
            if (amount <= 0) throw new DomainException("Winnings amount must be positive.");
            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            _transactions.Add(VoteTransaction.ForH2HWin(LeagueId, UserId, amount, matchId));
        }
    }
}
