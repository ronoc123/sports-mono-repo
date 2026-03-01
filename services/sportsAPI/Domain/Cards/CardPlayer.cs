using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Cards;

/// <summary>
/// A tradeable player card entry. Scoped to a league.
/// Completely independent of the Active Roster Player entity (no FK dependency).
/// Rarity tier is auto-assigned from RarityTierConfig — never set manually.
/// </summary>
public sealed class CardPlayer : Aggregate<Guid>
{
    internal CardPlayer() { } // EF

    public Guid LeagueId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Position { get; private set; } = default!;
    public int OverallRating { get; private set; }

    /// <summary>Auto-assigned based on OverallRating against RarityTierConfig thresholds.</summary>
    public string RarityTier { get; private set; } = default!;

    public static CardPlayer Create(
        Guid leagueId,
        string name,
        string position,
        int overallRating,
        string rarityTier)
    {
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(position);
        ArgumentException.ThrowIfNullOrWhiteSpace(rarityTier);
        if (overallRating < 0 || overallRating > 99)
            throw new DomainException("Overall rating must be between 0 and 99.");

        return new CardPlayer
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            Name = name.Trim(),
            Position = position.Trim(),
            OverallRating = overallRating,
            RarityTier = rarityTier,
        };
    }

    public void Update(string name, string position, int overallRating, string rarityTier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(position);
        ArgumentException.ThrowIfNullOrWhiteSpace(rarityTier);
        if (overallRating < 0 || overallRating > 99)
            throw new DomainException("Overall rating must be between 0 and 99.");

        Name = name.Trim();
        Position = position.Trim();
        OverallRating = overallRating;
        RarityTier = rarityTier;
    }
}
