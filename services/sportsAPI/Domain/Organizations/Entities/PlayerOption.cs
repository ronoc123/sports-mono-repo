

namespace Domain.Organizations.Entities
{
    public class PlayerOption : Entity<PlayerOptionId>
    {
        // Private setters to enforce business rules
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int Votes { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public PlayerId PlayerId { get; private set; } = null!;
        public OrganizationId OrganizationId { get; private set; } = null!;

        // Business logic properties
        public bool IsActive => DateTime.UtcNow < ExpiresAt;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsPopular => Votes >= 100; // Popular if has 100+ votes
        public bool IsTrending => Votes >= 50 && IsActive; // Trending if active with 50+ votes
        public TimeSpan TimeRemaining => IsActive ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero;
        public int DaysRemaining => IsActive ? (int)Math.Ceiling(TimeRemaining.TotalDays) : 0;

        // Parameterless constructor for EF Core
        internal PlayerOption() { }

        // Private constructor - use factory methods instead
        private PlayerOption(string title, string description, PlayerId playerId, OrganizationId organizationId, DateTime? expiresAt = null)
        {
            Id = PlayerOptionId.Of(Guid.NewGuid());
            Title = title;
            Description = description;
            Votes = 0; // Start with 0 votes
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMonths(1); // Default 1 month expiry
            PlayerId = playerId;
            OrganizationId = organizationId;
        }

        // Factory method with business logic validation
        public static PlayerOption Create(string title, string description, PlayerId playerId, OrganizationId organizationId, DateTime? expiresAt = null)
        {
            // Business rule validations
            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
            ArgumentNullException.ThrowIfNull(playerId, nameof(playerId));
            ArgumentNullException.ThrowIfNull(organizationId, nameof(organizationId));

            if (title.Length > 200)
                throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));

            if (description.Length > 1000)
                throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future", nameof(expiresAt));

            if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow.AddYears(1))
                throw new ArgumentException("Expiry date cannot be more than 1 year in the future", nameof(expiresAt));

            return new PlayerOption(title, description, playerId, organizationId, expiresAt);
        }

        // Domain methods for business operations
        public void UpdateDetails(string title, string description)
        {
            if (IsExpired)
                throw new InvalidOperationException("Cannot update expired player option");

            ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
            ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

            if (title.Length > 200)
                throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));

            if (description.Length > 1000)
                throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

            Title = title;
            Description = description;
        }

        public void ExtendExpiry(DateTime newExpiryDate)
        {
            if (newExpiryDate <= DateTime.UtcNow)
                throw new ArgumentException("New expiry date must be in the future", nameof(newExpiryDate));

            if (newExpiryDate <= ExpiresAt)
                throw new ArgumentException("New expiry date must be later than current expiry", nameof(newExpiryDate));

            if (newExpiryDate > DateTime.UtcNow.AddYears(1))
                throw new ArgumentException("Expiry date cannot be more than 1 year in the future", nameof(newExpiryDate));

            ExpiresAt = newExpiryDate;
        }

        public void AddVote()
        {
            if (IsExpired)
                throw new InvalidOperationException("Cannot vote on expired player option");

            Votes++;
        }

        public void RemoveVote()
        {
            if (IsExpired)
                throw new InvalidOperationException("Cannot remove vote from expired player option");

            if (Votes <= 0)
                throw new InvalidOperationException("Cannot remove vote when vote count is already zero");

            Votes--;
        }

        public void ExpireNow()
        {
            if (IsExpired)
                throw new InvalidOperationException("Player option is already expired");

            ExpiresAt = DateTime.UtcNow;
        }

        // Business logic methods
        public string GetPopularityLevel()
        {
            return Votes switch
            {
                >= 1000 => "Viral",
                >= 500 => "Very Popular",
                >= 100 => "Popular",
                >= 50 => "Trending",
                >= 10 => "Active",
                _ => "New"
            };
        }

        public decimal GetEngagementScore()
        {
            if (!IsActive) return 0;

            var ageInDays = (DateTime.UtcNow - (CreatedAt ?? DateTime.UtcNow)).TotalDays;
            if (ageInDays <= 0) return 0;

            // Engagement score = votes per day
            return (decimal)(Votes / ageInDays);
        }

        public bool ShouldPromote()
        {
            // Business rule: Promote if trending and has good engagement
            return IsTrending && GetEngagementScore() > 5;
        }
    }
}

