using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Cards;

/// <summary>
/// A card owned by a fan. Created when a pack is opened.
/// Serves as both ownership record and pull log entry.
/// </summary>
public sealed class UserCard : Aggregate<Guid>
{
    internal UserCard() { } // EF

    public Guid CardPackId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CardPlayerId { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid LeagueId { get; private set; }
    public string RarityTier { get; private set; } = default!;
    public string PullSeed { get; private set; } = default!;
    public bool IsListed { get; private set; }

    // Navigation (no cascade — card player is independent data)
    public CardPlayer? CardPlayer { get; private set; }

    public static UserCard Create(
        Guid cardPackId,
        Guid userId,
        Guid cardPlayerId,
        Guid orgId,
        Guid leagueId,
        string rarityTier,
        string pullSeed)
    {
        if (cardPackId == Guid.Empty) throw new DomainException("CardPackId is required.");
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (cardPlayerId == Guid.Empty) throw new DomainException("CardPlayerId is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(rarityTier);
        ArgumentException.ThrowIfNullOrWhiteSpace(pullSeed);

        return new UserCard
        {
            Id = Guid.NewGuid(),
            CardPackId = cardPackId,
            UserId = userId,
            CardPlayerId = cardPlayerId,
            OrgId = orgId,
            LeagueId = leagueId,
            RarityTier = rarityTier,
            PullSeed = pullSeed,
            IsListed = false,
        };
    }

    public void MarkAsListed() => IsListed = true;
    public void MarkAsUnlisted() => IsListed = false;
}
