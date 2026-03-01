using Domain.Cards;

namespace Application.Cards.Services;

/// <summary>
/// Executes weighted-random card pulls using pull-weight basis points
/// configured per rarity tier. Each call produces a reproducible seed
/// that is stored in the pull log for auditability.
/// </summary>
public sealed class RarityEngine
{
    public const int CardsPerPack = 5;

    /// <summary>
    /// A bucket groups a rarity tier's config with all eligible CardPlayers.
    /// </summary>
    public record Bucket(RarityTierConfig Config, IReadOnlyList<CardPlayer> Cards);

    /// <summary>
    /// Pull <paramref name="count"/> cards from the provided weighted buckets.
    /// </summary>
    /// <param name="buckets">One bucket per rarity tier, each with its cards.</param>
    /// <param name="count">Number of cards to pull (default 5).</param>
    /// <returns>Seed string and the drawn (card, rarityTier) pairs.</returns>
    public (string Seed, IReadOnlyList<(CardPlayer Card, string RarityTier)> Results) Pull(
        IReadOnlyList<Bucket> buckets,
        int count = CardsPerPack)
    {
        var seed = Guid.NewGuid().ToString("N")[..12];
        var rng = new Random(seed.GetHashCode());

        var totalBps = buckets.Sum(b => b.Config.PullWeightBps);
        var results = new List<(CardPlayer, string)>(count);

        for (var i = 0; i < count; i++)
        {
            var roll = rng.Next(0, totalBps);
            var cumulative = 0;
            var drawn = false;

            foreach (var bucket in buckets)
            {
                cumulative += bucket.Config.PullWeightBps;
                if (roll < cumulative && bucket.Cards.Count > 0)
                {
                    var card = bucket.Cards[rng.Next(0, bucket.Cards.Count)];
                    results.Add((card, bucket.Config.RarityName));
                    drawn = true;
                    break;
                }
            }

            // Fallback: if a bucket had 0 cards (edge case), pick from any non-empty bucket
            if (!drawn)
            {
                var fallback = buckets.FirstOrDefault(b => b.Cards.Count > 0);
                if (fallback is not null)
                {
                    var card = fallback.Cards[rng.Next(0, fallback.Cards.Count)];
                    results.Add((card, fallback.Config.RarityName));
                }
            }
        }

        return (seed, results);
    }
}
