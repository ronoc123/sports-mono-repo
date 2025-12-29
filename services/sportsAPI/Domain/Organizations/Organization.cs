namespace Domain.Organizations
{
    public class Organization : Aggregate<OrganizationId>
    {
        internal Organization() { }

        // Identity / associations
        public LeagueId LeagueId { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;

        // Team metadata
        public string? TeamId { get; private set; }
        public string? TeamName { get; private set; }
        public string? TeamShortName { get; private set; }
        public int? FormedYear { get; private set; }
        public string? Sport { get; private set; }

        // Owned value objects (mapped in Infrastructure)
        public Venue Venue { get; private set; } = null!;
        public MediaAssets MediaAssets { get; private set; } = null!;
        public SocialLinks SocialLinks { get; private set; } = null!;
        public TeamColors TeamColors { get; private set; } = null!;

        public string? Description { get; private set; }


        // Factory
        public static Organization Create(
            LeagueId leagueId,
            string name,
            string? teamId,
            string? teamName,
            string? teamShortName,
            int? formedYear,
            string? sport,
            string? stadium,
            string? location,
            int? capacity,
            string? badgeUrl,
            string? logoUrl,
            string? fanart1Url,
            string? fanart2Url,
            string? fanart3Url,
            string? website,
            string? facebook,
            string? twitter,
            string? instagram,
            string? color1,
            string? color2,
            string? color3,
            string? description)
        {
            ArgumentNullException.ThrowIfNull(leagueId);
            ArgumentException.ThrowIfNullOrEmpty(name);

            var venue = new Venue(
                stadium ?? string.Empty,
                location ?? string.Empty,
                capacity ?? 0);

            var mediaAssets = new MediaAssets(
                badgeUrl ?? string.Empty,
                logoUrl ?? string.Empty,
                fanart1Url ?? string.Empty,
                fanart2Url ?? string.Empty,
                fanart3Url ?? string.Empty);

            var socialLinks = new SocialLinks(
                website ?? string.Empty,
                facebook ?? string.Empty,
                twitter ?? string.Empty,
                instagram ?? string.Empty);

            var teamColors = new TeamColors(
                color1 ?? string.Empty,
                color2 ?? string.Empty,
                color3 ?? string.Empty);

            var org = new Organization
            {
                Id = OrganizationId.Of(Guid.NewGuid()),
                LeagueId = leagueId,
                CreatedAt = DateTime.UtcNow
            };

            org.Rename(name);
            org.UpdateTeamInfo(teamId, teamName, teamShortName, sport);
            org.SetFormedYear(formedYear);
            org.SetVenue(venue);
            org.SetMediaAssets(mediaAssets);
            org.SetSocialLinks(socialLinks);
            org.SetTeamColors(teamColors);
            org.SetDescription(description);

            return org;
        }

        // Mutations (Organization-only invariants)

        public void Rename(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            var trimmed = name.Trim();

            if (trimmed.Length > 200)
                throw new ArgumentException("Organization name cannot exceed 200 characters.", nameof(name));

            if (string.Equals(Name, trimmed, StringComparison.OrdinalIgnoreCase))
                return;

            Name = trimmed;
            // AddEvent(new OrganizationRenamed(Id, Name));
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

            TeamId = teamId;
            TeamName = teamName;
            TeamShortName = teamShortName;
            Sport = sport;
            // AddEvent(new OrganizationTeamInfoUpdated(Id, TeamId, TeamName, TeamShortName, Sport));
        }

        public void SetFormedYear(int? formedYear)
        {
            if (formedYear.HasValue && (formedYear < 1800 || formedYear > DateTime.UtcNow.Year + 1))
                throw new ArgumentOutOfRangeException(nameof(formedYear), "Formed year is out of range.");

            FormedYear = formedYear;
            // AddEvent(new OrganizationFormedYearUpdated(Id, FormedYear));
        }

        public void SetVenue(Venue venue)
        {
            ArgumentNullException.ThrowIfNull(venue);
            Venue = venue;
            // AddEvent(new OrganizationVenueUpdated(Id));
        }

        public void SetMediaAssets(MediaAssets mediaAssets)
        {
            ArgumentNullException.ThrowIfNull(mediaAssets);
            MediaAssets = mediaAssets;
            // AddEvent(new OrganizationMediaAssetsUpdated(Id));
        }

        public void SetSocialLinks(SocialLinks socialLinks)
        {
            ArgumentNullException.ThrowIfNull(socialLinks);
            SocialLinks = socialLinks;
            // AddEvent(new OrganizationSocialLinksUpdated(Id));
        }

        public void SetTeamColors(TeamColors colors)
        {
            ArgumentNullException.ThrowIfNull(colors);
            TeamColors = colors;
            // AddEvent(new OrganizationTeamColorsUpdated(Id));
        }

        public void SetDescription(string? description) => Description = description;

    }

    // Domain events you can keep if you’re publishing them
    public sealed record OrganizationCreated(OrganizationId OrganizationId, LeagueId LeagueId, string Name) : DomainEvent;
    public sealed record OrganizationRenamed(OrganizationId OrganizationId, string Name);
    public sealed record OrganizationTeamInfoUpdated(OrganizationId OrganizationId, string? TeamId, string? TeamName, string? TeamShortName, string? Sport);
    public sealed record OrganizationFormedYearUpdated(OrganizationId OrganizationId, int? FormedYear);
    public sealed record OrganizationVenueUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationMediaAssetsUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationSocialLinksUpdated(OrganizationId OrganizationId);
    public sealed record OrganizationTeamColorsUpdated(OrganizationId OrganizationId);
}
