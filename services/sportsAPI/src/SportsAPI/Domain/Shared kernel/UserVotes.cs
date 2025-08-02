
namespace Domain.User.Entities
{
    public class UserVotes : Entity<UserVotesId>
    {
        public OrganizationId OrganizationId { get; private set; } = null!;
        public int VotesRemaining { get; private set; }

        internal UserVotes() { }

        public UserVotes(OrganizationId organizationId, int votesRemaining)
        {
            Id = UserVotesId.Of(Guid.NewGuid());
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
