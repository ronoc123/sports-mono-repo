
namespace Domain.VoteAccount;

public sealed class VoteAccount : Aggregate<VoteAccountId>
{
  // EF needs a parameterless ctor
  internal VoteAccount() { }

  // Convenience accessors (optional, derived from Id)
  public OrganizationId OrgId { get; private set; } = default!;
  public UserId UserId { get; private set; } = default!;

  public long Balance { get; private set; }
  /// <summary>Optimistic concurrency token (monotonically increasing)</summary>
  public long Version { get; private set; }

  public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

  private VoteAccount(VoteAccountId id, long initialBalance = 0)
  {
    Id = id;
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
    //AddEvent(new VotesCredited(Id, amount, reason, refId, OccurredAt: DateTimeOffset.UtcNow));
  }

  public void Spend(long amount, string reason = "vote_spend", long? refId = null)
  {
    if (amount <= 0) throw new DomainExceptions("Spend amount must be positive.");
    if (Balance < amount) throw new DomainExceptions("Insufficient balance.");
    Balance -= amount;
    touch();
    //AddEvent(new VotesSpent(Id, amount, reason, refId, OccurredAt: DateTimeOffset.UtcNow));
  }

  private void touch()
  {
    Version++;
    UpdatedAt = DateTime.UtcNow;
  }
}
