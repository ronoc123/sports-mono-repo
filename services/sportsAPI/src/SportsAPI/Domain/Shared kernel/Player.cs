


namespace Domain.Organizations.Entities
{
    public class Player : Entity<PlayerId>
    {
        // Private setters to enforce business rules
        public string Name { get; private set; } = string.Empty;
        public string Position { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; } = string.Empty;
        public DateTime UpdatedAt { get; private set; }
        public int Age { get; private set; }
        public OrganizationId? OrganizationId { get; private set; }
        public LeagueId LeagueId { get; private set; } = null!;

        // Business logic properties
        public bool IsActive => Age >= 16 && Age <= 50; // Active playing age range
        public bool IsVeteran => Age >= 35;
        public bool IsYoungPlayer => Age <= 23;

        // Parameterless constructor for EF Core
        internal Player() { }

        // Private constructor - use factory methods instead
        private Player(string name, string position, string imageUrl, int age, LeagueId leagueId, OrganizationId? organizationId = null)
        {
            Id = PlayerId.Of(Guid.NewGuid());
            Name = name;
            Position = position;
            ImageUrl = imageUrl;
            Age = age;
            OrganizationId = organizationId;
            UpdatedAt = DateTime.UtcNow;
            LeagueId = leagueId;
            CreatedAt = DateTime.UtcNow;
        }

        // Factory method with business logic validation
        public static Player Create(string name, string position, string imageUrl, int age, LeagueId leagueId, OrganizationId? organizationId = null)
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

        // Domain methods for business operations
        public void UpdatePlayerInfo(string name, string position, string imageUrl, int age)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            ArgumentException.ThrowIfNullOrWhiteSpace(position, nameof(position));

            if (name.Length > 200)
                throw new ArgumentException("Player name cannot exceed 200 characters", nameof(name));

            if (position.Length > 100)
                throw new ArgumentException("Position cannot exceed 100 characters", nameof(position));

            if (age < 16 || age > 50)
                throw new ArgumentException("Player age must be between 16 and 50", nameof(age));

            if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                throw new ArgumentException("Image URL must be a valid URL", nameof(imageUrl));

            Name = name;
            Position = position;
            ImageUrl = imageUrl;
            Age = age;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignToOrganization(OrganizationId organizationId)
        {
            ArgumentNullException.ThrowIfNull(organizationId, nameof(organizationId));

            OrganizationId = organizationId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveFromOrganization()
        {
            OrganizationId = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void TransferToLeague(LeagueId newLeagueId)
        {
            ArgumentNullException.ThrowIfNull(newLeagueId, nameof(newLeagueId));

            if (LeagueId == newLeagueId)
                throw new InvalidOperationException("Player is already in this league");

            LeagueId = newLeagueId;
            OrganizationId = null; // Remove from organization when transferring leagues
            UpdatedAt = DateTime.UtcNow;
        }

        // Business logic methods
        public bool CanPlayInPosition(string position)
        {
            // Business logic: Some positions have age restrictions
            return position.ToLower() switch
            {
                "goalkeeper" => Age >= 18, // Goalkeepers need experience
                "striker" => Age <= 40,    // Strikers need speed
                _ => IsActive
            };
        }

        public decimal GetMarketValue()
        {
            // Simple market value calculation based on age
            return Age switch
            {
                <= 20 => 50000m,  // Young talent
                <= 25 => 100000m, // Prime young player
                <= 30 => 80000m,  // Experienced player
                <= 35 => 40000m,  // Veteran
                _ => 20000m       // Senior player
            };
        }
    }
}
