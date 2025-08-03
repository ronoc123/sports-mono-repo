
using Domain.User.Entities;
using Domain.Users.Entities;

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
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? Phone { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public string? Bio { get; private set; }
        public string? Avatar { get; private set; }
        public UserPreferences Preferences { get; private set; } = new();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public static User Create(Guid id, string email, string userName, string firstName, string lastName)
        {
            return new User
            {
                Id = UserId.Of(id),
                Email = email,
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string firstName, string lastName, string? phone = null,
            DateTime? dateOfBirth = null, string? bio = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(firstName);
            ArgumentException.ThrowIfNullOrEmpty(lastName);

            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            DateOfBirth = dateOfBirth;
            Bio = bio;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAvatar(string avatarUrl)
        {
            Avatar = avatarUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePreferences(UserPreferences preferences)
        {
            ArgumentNullException.ThrowIfNull(preferences);
            Preferences = preferences;
            UpdatedAt = DateTime.UtcNow;
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
