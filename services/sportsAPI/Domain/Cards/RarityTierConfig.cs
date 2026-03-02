using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Cards;

/// <summary>
/// GM-configurable rarity tier thresholds and pull weights.
/// Scoped to a league — each league can tune its own economy.
/// </summary>
public sealed class RarityTierConfig : Aggregate<Guid>
{
    internal RarityTierConfig() { } // EF

    public Guid LeagueId { get; private set; }
    public string RarityName { get; private set; } = default!;
    public int RatingMin { get; private set; }
    public int RatingMax { get; private set; }

    /// <summary>
    /// Pull weight in basis points. Sum across all tiers for an org must equal 10,000 (100%).
    /// </summary>
    public int PullWeightBps { get; private set; }

    public static RarityTierConfig Create(
        Guid leagueId,
        string rarityName,
        int ratingMin,
        int ratingMax,
        int pullWeightBps)
    {
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(rarityName);
        if (ratingMin < 0 || ratingMax > 99 || ratingMin >= ratingMax)
            throw new DomainException("Rating range must be valid (0–99, min < max).");
        if (pullWeightBps <= 0)
            throw new DomainException("Pull weight must be positive.");

        return new RarityTierConfig
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            RarityName = rarityName.Trim(),
            RatingMin = ratingMin,
            RatingMax = ratingMax,
            PullWeightBps = pullWeightBps,
        };
    }

    public void Update(int ratingMin, int ratingMax, int pullWeightBps)
    {
        if (ratingMin < 0 || ratingMax > 99 || ratingMin >= ratingMax)
            throw new DomainException("Rating range must be valid (0–99, min < max).");
        if (pullWeightBps <= 0)
            throw new DomainException("Pull weight must be positive.");

        RatingMin = ratingMin;
        RatingMax = ratingMax;
        PullWeightBps = pullWeightBps;
    }
}
