
using Domain.Organizations;
using Domain.Organizations.Entities;

namespace Domain.Leagues
{
    public class League : Aggregate<LeagueId>
    {
        internal League() { }

        public string Name { get; private set; } = string.Empty;

        private readonly List<Player> _players = new();
        public IReadOnlyList<Player> Players => _players.AsReadOnly();

        private readonly List<Organization> _organization = new();
        public IReadOnlyList<Organization> Organization => _organization.AsReadOnly();

        public static League Create(LeagueId id, string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            if (name.Length > 200)
                throw new ArgumentException("League name cannot exceed 200 characters", nameof(name));

            return new League
            {
                Id = id,
                Name = name,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Domain method to update league name
        public void UpdateName(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            if (name.Length > 200)
                throw new ArgumentException("League name cannot exceed 200 characters", nameof(name));

            Name = name;
        }

        // Domain method to add player using the Player factory method
        public Player AddPlayer(string name, string position, string imageUrl, int age, OrganizationId? organizationId = null)
        {
            var player = Player.Create(name, position, imageUrl, age, this.Id, organizationId);
            _players.Add(player);

            // TODO: Add domain event for player added to league
            return player;
        }

        // Domain method to remove player
        public void RemovePlayer(PlayerId playerId)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
                throw new InvalidOperationException("Player not found in league");

            _players.Remove(player);
            // TODO: Add domain event for player removed from league
        }

        // Domain method to add organization
        public void AddOrganization(Organization organization)
        {
            ArgumentNullException.ThrowIfNull(organization, nameof(organization));

            if (organization.LeagueId != this.Id)
                throw new InvalidOperationException("Organization must belong to this league");

            if (_organization.Any(o => o.Name == organization.Name))
                throw new InvalidOperationException("Organization with this name already exists in the league");

            _organization.Add(organization);
            // TODO: Add domain event for organization added to league
        }

        // Domain method to remove organization
        public void RemoveOrganization(OrganizationId organizationId)
        {
            var organization = _organization.FirstOrDefault(o => o.Id == organizationId);
            if (organization == null)
                throw new InvalidOperationException("Organization not found in league");

            if (organization.HasActivePlayerOptions())
                throw new InvalidOperationException("Cannot remove organization with active player options");

            _organization.Remove(organization);
            // TODO: Add domain event for organization removed from league
        }

        // Business logic methods
        public bool CanAddOrganization()
        {
            // Business rule: Maximum 20 organizations per league
            return _organization.Count < 20;
        }

        public int GetTotalPlayers()
        {
            return _players.Count;
        }

        public int GetActivePlayers()
        {
            return _players.Count(p => p.IsActive);
        }

        public int GetTotalOrganizations()
        {
            return _organization.Count;
        }

        public decimal GetAveragePlayerAge()
        {
            if (!_players.Any()) return 0;
            return (decimal)_players.Average(p => p.Age);
        }

        public decimal GetTotalMarketValue()
        {
            return _players.Sum(p => p.GetMarketValue());
        }

        public IEnumerable<Player> GetYoungPlayers()
        {
            return _players.Where(p => p.IsYoungPlayer);
        }

        public IEnumerable<Player> GetVeteranPlayers()
        {
            return _players.Where(p => p.IsVeteran);
        }

        public Organization? GetMostActiveOrganization()
        {
            return _organization
                .OrderByDescending(o => o.GetTotalVotes())
                .FirstOrDefault();
        }

        public bool HasOrganizationWithName(string name)
        {
            return _organization.Any(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool CanDelete()
        {
            // Business rule: Can only delete league if no organizations have active player options
            return !_organization.Any(o => o.HasActivePlayerOptions());
        }
    }
}
