using Domain.Abstractions;

namespace Domain.H2H;

/// <summary>
/// Records which CardOwner was placed in each slot of a fan's H2H squad.
/// </summary>
public sealed class H2HSquadCard : Aggregate<Guid>
{
    internal H2HSquadCard() { } // EF

    public Guid MatchId { get; private set; }
    public Guid CardOwnerId { get; private set; }

    /// <summary>Zero-based slot index (0–4).</summary>
    public int SlotIndex { get; private set; }

    public static H2HSquadCard Create(Guid matchId, Guid cardOwnerId, int slotIndex)
    {
        return new H2HSquadCard
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            CardOwnerId = cardOwnerId,
            SlotIndex = slotIndex,
        };
    }
}
