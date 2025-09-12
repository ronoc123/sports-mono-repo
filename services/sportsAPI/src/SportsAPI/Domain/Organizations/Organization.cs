using Domain.Organizations.Entities;
using Domain.ValueObjects.ConcreteTypes;


namespace Domain.Organizations
{
    public class Organization : Aggregate<OrganizationId>
    {
        // Parameterless constructor for EF Core
        internal Organization() { }

        internal Organization(
            OrganizationId id,
            LeagueId leagueId,
            string name,
            string? teamId,
            string? teamName,
            string? teamShortName,
            int? formedYear,
            string? sport,
            Venue venue,
            MediaAssets mediaAssets,
            SocialLinks socialLinks,
            TeamColors teamColors,
            string? description)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(leagueId);
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(venue);
            ArgumentNullException.ThrowIfNull(mediaAssets);
            ArgumentNullException.ThrowIfNull(socialLinks);
            ArgumentNullException.ThrowIfNull(teamColors);

            LeagueId = leagueId;
            Name = name;
            TeamId = teamId;
            TeamName = teamName;
            TeamShortName = teamShortName;
            FormedYear = formedYear;
            Sport = sport;
            Venue = venue;
            MediaAssets = mediaAssets;
            SocialLinks = socialLinks;
            TeamColors = teamColors;
            Description = description;
            CreatedAt = DateTime.Now;
        }

        private readonly List<PlayerOption> _playerOptions = new();
        public IReadOnlyList<PlayerOption> PlayerOptions => _playerOptions.AsReadOnly();
        private readonly List<Code> _codes = new();
        public IReadOnlyList<Code> Codes => _codes.AsReadOnly();
        private readonly List<Player> _players = new();
        public IReadOnlyList<Player> Player => _players.AsReadOnly();
        public LeagueId LeagueId { get; private set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? TeamId { get; private set; }
        public string? TeamName { get; private set; }
        public string? TeamShortName { get; private set; }
        public int? FormedYear { get; set; }
        public string? Sport { get; private set; }
        public Venue Venue { get; private set; } = null!;
        public MediaAssets MediaAssets { get; private set; } = null!;
        public SocialLinks SocialLinks { get; private set; } = null!;
        public TeamColors TeamColors { get; private set; } = null!;
        public string? Description { get; set; }

        // Factory method to create organization
        public static Organization Create(
            OrganizationId id,
            LeagueId leagueId,
            string name,
            string? teamId,
            string? teamName,
            string? teamShortName,
            int? formedYear,
            string? sport,
            Venue venue,
            MediaAssets mediaAssets,
            SocialLinks socialLinks,
            TeamColors teamColors,
            string? description)
        {
            return new Organization
            {
                Id = id,
                LeagueId = leagueId,
                Name = name,
                TeamId = teamId,
                TeamName = teamName,
                TeamShortName = teamShortName,
                FormedYear = formedYear,
                Sport = sport,
                Venue = venue,
                MediaAssets = mediaAssets,
                SocialLinks = socialLinks,
                TeamColors = teamColors,
                Description = description,
                CreatedAt = DateTime.UtcNow,
            };
        }

        public void UpdateTeamInfo(string? teamId, string? teamName, string? teamShortName, string? sport)
        {
            if (!string.IsNullOrEmpty(teamId) && teamId.Length > 50)
                throw new ArgumentException("Team ID cannot exceed 50 characters", nameof(teamId));

            if (!string.IsNullOrEmpty(teamName) && teamName.Length > 200)
                throw new ArgumentException("Team name cannot exceed 200 characters", nameof(teamName));

            if (!string.IsNullOrEmpty(teamShortName) && teamShortName.Length > 10)
                throw new ArgumentException("Team short name cannot exceed 10 characters", nameof(teamShortName));

            if (!string.IsNullOrEmpty(sport) && sport.Length > 100)
                throw new ArgumentException("Sport cannot exceed 100 characters", nameof(sport));

            TeamName = teamName;
            TeamShortName = teamShortName;
            Sport = sport;
        }

        public PlayerOption CreatePlayerOption(string title, string description, PlayerId playerId, DateTime? expiresAt = null)
        {

            var playerOption = PlayerOption.Create(title, description, playerId, this.Id, expiresAt);
            _playerOptions.Add(playerOption);

            // TODO: Add domain event for player option created
            return playerOption;
        }

        public void RemovePlayerOption(PlayerOptionId playerOptionId)
        {
            var playerOption = _playerOptions.FirstOrDefault(po => po.Id == playerOptionId);
            if (playerOption == null)
                throw new InvalidOperationException("Player option not found");

            if (playerOption.IsActive)
                throw new InvalidOperationException("Cannot remove active player option. Expire it first.");

            _playerOptions.Remove(playerOption);
            // TODO: Add domain event for player option removed
        }

        public void AddPlayer(Player player)
        {
            ArgumentNullException.ThrowIfNull(player, nameof(player));

            if (player.LeagueId != this.LeagueId)
                throw new InvalidOperationException("Player must be in the same league as the organization");

            _players.Add(player);
            player.AssignToOrganization(this.Id);
            // TODO: Add domain event for player added
        }

        public void RemovePlayer(PlayerId playerId)
        {
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
                throw new InvalidOperationException("Player not found in organization");

            _players.Remove(player);
            player.RemoveFromOrganization();
            // TODO: Add domain event for player removed
        }

        // TODO: Add methods to update value objects (Venue, MediaAssets, SocialLinks, TeamColors)
        // when they are properly configured in EF Core
    }

    // --- Domain Events TODO
    public sealed record OrganizationCreated(OrganizationId OrganizationId, LeagueId LeagueId, string Name);
    public sealed record OrganizationRenamed(OrganizationId OrganizationId, string Name);
    public sealed record OrganizationTeamInfoUpdated(OrganizationId OrganizationId, string? TeamId, string? TeamName, string? TeamShortName, string? Sport);
    public sealed record OrganizationFormedYearUpdated(OrganizationId OrganizationId, int? FormedYear);
    public sealed record OrganizationVenueUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationMediaAssetsUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationSocialLinksUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationTeamColorsUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationDescriptionUpdated(OrganizationId OrganizationId);

    public sealed record PlayerOptionCreated(OrganizationId OrganizationId, PlayerOptionId PlayerOptionId, PlayerId PlayerId);
    public sealed record PlayerOptionRemoved(OrganizationId OrganizationId, PlayerOptionId PlayerOptionId);
    public sealed record PlayerAddedToOrganization(OrganizationId OrganizationId, PlayerId PlayerId);
    public sealed record PlayerRemovedFromOrganization(OrganizationId OrganizationId, PlayerId PlayerId);
}
