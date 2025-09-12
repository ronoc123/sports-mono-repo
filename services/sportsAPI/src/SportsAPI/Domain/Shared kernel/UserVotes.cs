
namespace Domain.User.Entities
{
  public class UserVotes : Entity<Guid>
  {
    public OrganizationId OrganizationId { get; private set; } = null!;
    public int VotesRemaining { get; private set; }

    internal UserVotes() { }

    public UserVotes(OrganizationId organizationId, int votesRemaining)
    {
      Id = Guid.NewGuid();
      OrganizationId = organizationId;
      VotesRemaining = votesRemaining;
    }

    public void UseVote()
    {
      if (VotesRemaining <= 0)
        throw new InvalidOperationException("No votes remaining");

      VotesRemaining--;
    }

    internal void AddVotes(int votes)
    {
      if (votes <= 0)
        throw new ArgumentException("Votes must be positive", nameof(votes));

      VotesRemaining += votes;
    }
  }
}
