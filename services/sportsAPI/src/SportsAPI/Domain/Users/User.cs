
using Domain.User.Entities;

namespace Domain.Users
{
    public class User : Aggregate<UserId>
    {
        internal User()
        {
        }

        private readonly List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();
        private readonly List<Code> _codes = new();
        public IReadOnlyList<Code> Codes => _codes.AsReadOnly();
        private readonly List<UserVotes> _votesAvailable = new();
        public IReadOnlyList<UserVotes> VotesAvailable => _votesAvailable.AsReadOnly();

        public string Email { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;

        public static User Create(Guid id, string email, string userName)
        {
            return new User
            {
                Id = UserId.Of(id),
                Email = email,
                UserName = userName
            };
        }

        public void UpdateEmail(string email)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);
            Email = email;
        }

        public void UpdateUserName(string userName)
        {
            ArgumentException.ThrowIfNullOrEmpty(userName);
            UserName = userName;
        }

        public void AddVote(Vote vote)
        {
            _votes.Add(vote);
        }

        public void RedeemCode(Code code)
        {
            _codes.Add(code);
        }

        public void AddVotesForOrganization(OrganizationId organizationId, int votes)
        {
            var existing = _votesAvailable.FirstOrDefault(x => x.OrganizationId == organizationId);
            if (existing != null)
            {
                existing.AddVotes(votes);
            }
            else
            {
                _votesAvailable.Add(new UserVotes(organizationId, votes));
            }
        }

        public void UseVoteForOrganization(OrganizationId organizationId)
        {
            var userVotes = _votesAvailable.FirstOrDefault(x => x.OrganizationId == organizationId);
            if (userVotes == null)
                throw new InvalidOperationException("User has no votes for this organization.");

            userVotes.UseVote();
        }
    }
}
