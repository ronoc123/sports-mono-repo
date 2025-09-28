
using BuildingBlocks.Exceptions;
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

        /// <summary>
        /// Rename this league, enforcing invariants.
        /// </summary>
        public void SetName(string name)
        {
          ArgumentException.ThrowIfNullOrEmpty(name);

          if (Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            return; // No change, ignore

          if (name.Length > 200)
            throw new DomainExceptions("League name cannot exceed 200 characters.");

          Name = name.Trim();
        }

        public void AddPlayer(Player player)
        {
          _players.Add(player);
        }

        public static Player CreatePlayer(string name, string position, string imageUrl, int age, LeagueId leagueId, OrganizationId? organizationId = null)
        {
          // Business rule validations
          ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
          ArgumentException.ThrowIfNullOrWhiteSpace(position, nameof(position));
          ArgumentNullException.ThrowIfNull(leagueId, nameof(leagueId));

          if (name.Length > 200)
            throw new ArgumentException("Player name cannot exceed 200 characters", nameof(name));

          if (position.Length > 100)
            throw new ArgumentException("Position cannot exceed 100 characters", nameof(position));

          if (age < 16 || age > 50)
            throw new ArgumentException("Player age must be between 16 and 50", nameof(age));

          if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Image URL must be a valid URL", nameof(imageUrl));

          return new Player(name, position, imageUrl, age, leagueId, organizationId);
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
