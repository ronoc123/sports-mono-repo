
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

        private readonly List<Organization> _organizations = new();
        public IReadOnlyList<Organization> Organizations => _organizations.AsReadOnly();

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

            if (_organizations.Any(o => o.Name == organization.Name))
                throw new InvalidOperationException("Organization with this name already exists in the league");

            _organizations.Add(organization);
            // TODO: Add domain event for organization added to league
        }

        // Domain method to remove organization
        public void RemoveOrganization(OrganizationId organizationId)
        {
            var organization = _organizations.FirstOrDefault(o => o.Id == organizationId);
            if (organization == null)
                throw new InvalidOperationException("Organization not found in league");

            _organizations.Remove(organization);
            // TODO: Add domain event for organization removed from league
        }


        public int TotalPlayers => _players.Count;
        public int ActivePlayers => _players.Count(p => p.IsActive);
        public int TotalOrganizations => _organizations.Count;
      }

      // Domain events TO DO
      public sealed record LeagueCreated(LeagueId LeagueId, string Name);
      public sealed record LeagueRenamed(LeagueId LeagueId, string Name);
      public sealed record PlayerAddedToLeague(LeagueId LeagueId, PlayerId PlayerId);
      public sealed record PlayerRemovedFromLeague(LeagueId LeagueId, PlayerId PlayerId);
      public sealed record OrganizationAddedToLeague(LeagueId LeagueId, OrganizationId OrganizationId);
      public sealed record OrganizationRemovedFromLeague(LeagueId LeagueId, OrganizationId OrganizationId);
}
