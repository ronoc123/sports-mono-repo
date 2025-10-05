


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
        internal Player(string name, string position, string imageUrl, int age, LeagueId leagueId, OrganizationId? organizationId = null)
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

        internal Player UpdatePlayerInfo(string name, string position, string imageUrl, int age)
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
            return this;
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
    }
}
