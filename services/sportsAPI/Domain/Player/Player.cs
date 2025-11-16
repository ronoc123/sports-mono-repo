namespace Domain.Player
{
    public class Player : Aggregate<PlayerId>
    {
        // Identity / associations
        public LeagueId LeagueId { get; private set; } = null!;
        public OrganizationId? OrganizationId { get; private set; }

        // Core profile
        public string Name { get; private set; } = string.Empty;
        public string Position { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; } = string.Empty;
        public int Age { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public bool IsActive => Age is >= 16 and <= 50;
        public bool IsVeteran => Age >= 35;
        public bool IsYoungPlayer => Age <= 23;

        internal Player() { } // for EF

        public static Player Create(
            PlayerId id,
            LeagueId leagueId,
            string name,
            string position,
            string imageUrl,
            int age,
            OrganizationId? organizationId = null)
        {
          ArgumentNullException.ThrowIfNull(id);
          ArgumentNullException.ThrowIfNull(leagueId);
          ValidateProfile(name, position, imageUrl, age);

          return new Player
          {
            Id = id,
            LeagueId = leagueId,
            OrganizationId = organizationId,
            Name = name.Trim(),
            Position = position.Trim(),
            ImageUrl = imageUrl ?? string.Empty,
            Age = age,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
          };
        }

        public void UpdateProfile(string name, string position, string imageUrl, int age)
        {
          ValidateProfile(name, position, imageUrl, age);

          Name = name.Trim();
          Position = position.Trim();
          ImageUrl = imageUrl ?? string.Empty;
          Age = age;
          touch();
        }

        public void AssignToOrganization(OrganizationId organizationId)
        {
          ArgumentNullException.ThrowIfNull(organizationId);
          OrganizationId = organizationId;
          touch();

          // Cross-AR rule like "org.LeagueId == this.LeagueId" should be enforced
          // in the application layer before calling this method.
        }

        public void RemoveFromOrganization()
        {
          OrganizationId = null;
          touch();
        }

        // (Optional) narrow updates if you prefer smaller methods:
        public void UpdateImage(string imageUrl)
        {
          if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Image URL must be a valid URL.", nameof(imageUrl));
          ImageUrl = imageUrl ?? string.Empty;
          touch();
        }

        private static void ValidateProfile(string name, string position, string imageUrl, int age)
        {
          ArgumentException.ThrowIfNullOrWhiteSpace(name);
          ArgumentException.ThrowIfNullOrWhiteSpace(position);

          if (name.Length > 200) throw new ArgumentException("Player name cannot exceed 200 characters.", nameof(name));
          if (position.Length > 100) throw new ArgumentException("Position cannot exceed 100 characters.", nameof(position));
          if (age < 16 || age > 50) throw new ArgumentException("Player age must be between 16 and 50.", nameof(age));
          if (!string.IsNullOrEmpty(imageUrl) && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Image URL must be a valid URL.", nameof(imageUrl));
        }

        private void touch()
        {
          UpdatedAt = DateTime.UtcNow;
        }
  }
}
