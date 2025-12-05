namespace Domain.PlayerOption
{

  using Domain.Shared_kernel;

  public class PlayerOption : Aggregate<PlayerOptionId>
  {
    public OrganizationId OrganizationId { get; private set; } = default!;
    public PlayerId PlayerId { get; private set; } = default!;

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }

    public long Votes { get; private set; }

    public bool IsActive => DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => !IsActive;
    public TimeSpan TimeRemaining => IsActive ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero;
    public int DaysRemaining => IsActive ? (int)Math.Ceiling(TimeRemaining.TotalDays) : 0;
    public bool IsPopular => Votes >= 100;
    public bool IsTrending => Votes >= 50 && IsActive;

    internal PlayerOption() { }

    private PlayerOption(
        PlayerOptionId id,
        string title,
        string description,
        PlayerId playerId,
        OrganizationId organizationId,
        DateTime? expiresAt)
    {
      Id = id;
      Title = title;
      Description = description;
      PlayerId = playerId;
      OrganizationId = organizationId;
      Votes = 0;
      CreatedAt = DateTime.UtcNow;
      ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMonths(1);
    }

    public static PlayerOption Create(
        string title,
        string description,
        PlayerId playerId,
        OrganizationId organizationId,
        DateTime? expiresAt = null)
    {
      ArgumentNullException.ThrowIfNull(playerId);
      ArgumentNullException.ThrowIfNull(organizationId);
      ValidateText(title, description);
      ValidateExpiry(expiresAt);

      return new PlayerOption(
          PlayerOptionId.Of(Guid.NewGuid()),
          title.Trim(),
          description.Trim(),
          playerId,
          organizationId,
          expiresAt);
    }

    public void UpdateDetails(string title, string description)
    {
      if (IsExpired) throw new InvalidOperationException("Cannot update an expired option.");
      ValidateText(title, description);

      Title = title.Trim();
      Description = description.Trim();
    }

    public void ExtendExpiry(DateTime newExpiryDate)
    {
      if (newExpiryDate <= DateTime.UtcNow)
        throw new ArgumentException("Expiry must be in the future.", nameof(newExpiryDate));
      if (newExpiryDate <= ExpiresAt)
        throw new ArgumentException("New expiry must be later than current expiry.", nameof(newExpiryDate));
      if (newExpiryDate > DateTime.UtcNow.AddYears(1))
        throw new ArgumentException("Expiry cannot be more than 1 year in the future.", nameof(newExpiryDate));

      ExpiresAt = newExpiryDate;
    }

    public void ExpireNow()
    {
      if (IsExpired) return;
      ExpiresAt = DateTime.UtcNow;
    }


    public void CastVote(UserId userId, SpendToken spend)
    {
      if (!IsActive)
        throw new InvalidOperationException("Voting is not open.");

      if (spend.PlayerOptionId != Id)
        throw new InvalidOperationException("Spend token not authorized for this option.");

      if (spend.OrgId != OrganizationId)
        throw new InvalidOperationException("Spend token not authorized for this organization.");

      if (spend.Amount <= 0)
        throw new InvalidOperationException("Vote amount must be positive.");

      Votes += spend.Amount;
    }

    // Helpers
    private static void ValidateText(string title, string description)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(title);
      ArgumentException.ThrowIfNullOrWhiteSpace(description);
      if (title.Length > 200) throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
      if (description.Length > 1000) throw new ArgumentException("Description cannot exceed 1000 characters.", nameof(description));
    }

    private static void ValidateExpiry(DateTime? expiresAt)
    {
      if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));
      if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow.AddYears(1))
        throw new ArgumentException("Expiry cannot be more than 1 year in the future.", nameof(expiresAt));
    }

    // Optional scoring helpers
    public string GetPopularityLevel() =>
        Votes switch
        {
          >= 1000 => "Viral",
          >= 500 => "Very Popular",
          >= 100 => "Popular",
          >= 50 => "Trending",
          >= 10 => "Active",
          _ => "New"
        };

    public decimal GetEngagementScore()
    {
      if (!IsActive) return 0;
      var started = CreatedAt ?? DateTime.UtcNow;
      var days = (DateTime.UtcNow - started).TotalDays;
      if (days <= 0) return 0;
      return (decimal)(Votes / days);
    }

    public bool ShouldPromote() => IsTrending && GetEngagementScore() > 5;
  }
}
