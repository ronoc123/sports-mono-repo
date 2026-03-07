using BuildingBlocks.Exceptions;

namespace Domain.Cards;

/// <summary>
/// Represents a specific copy of a CardPlayer owned by a fan.
/// One CardOwner row per card instance — a fan who pulls two copies of the same
/// CardPlayer will have two CardOwner rows.
/// IsListed = true while the card is on the auction marketplace.
/// </summary>
public sealed class CardOwner : Aggregate<Guid>
{
    internal CardOwner() { } // EF

    public Guid CardPlayerId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LeagueId { get; private set; }

    /// <summary>True while this card is listed on the marketplace (locked from H2H).</summary>
    public bool IsListed { get; private set; }

    public DateTime AcquiredAt { get; private set; }

    // Navigation property (optional, for includes)
    public CardPlayer? CardPlayer { get; private set; }

    public static CardOwner Create(Guid cardPlayerId, Guid userId, Guid leagueId)
    {
        if (cardPlayerId == Guid.Empty) throw new DomainException("CardPlayerId is required.");
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");

        return new CardOwner
        {
            Id = Guid.NewGuid(),
            CardPlayerId = cardPlayerId,
            UserId = userId,
            LeagueId = leagueId,
            IsListed = false,
            AcquiredAt = DateTime.UtcNow,
        };
    }

    public void MarkAsListed()
    {
        if (IsListed) throw new DomainException("Card is already listed.");
        IsListed = true;
    }

    public void MarkAsUnlisted()
    {
        IsListed = false;
    }

    /// <summary>Transfer ownership to a new user (auction settlement or buy now).</summary>
    public void TransferTo(Guid newUserId)
    {
        if (newUserId == Guid.Empty) throw new DomainException("NewUserId is required.");
        UserId = newUserId;
        IsListed = false;
        AcquiredAt = DateTime.UtcNow;
    }
}
