using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Marketplace;

/// <summary>
/// Tracks points held in escrow for an active bid on an auction listing.
/// Status: "held" → "released" | "settled"
/// </summary>
public sealed class PointsEscrow : Aggregate<Guid>
{
    internal PointsEscrow() { } // EF

    public Guid UserId { get; private set; }
    public Guid ListingId { get; private set; }
    public long HeldAmount { get; private set; }
    public string Status { get; private set; } = "held";

    public bool IsHeld => Status == "held";

    public static PointsEscrow Create(Guid userId, Guid listingId, long heldAmount)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (listingId == Guid.Empty) throw new DomainException("ListingId is required.");
        if (heldAmount <= 0) throw new DomainException("Held amount must be greater than zero.");

        return new PointsEscrow
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ListingId = listingId,
            HeldAmount = heldAmount,
            Status = "held",
        };
    }

    public void Release()
    {
        if (Status != "held") throw new DomainException("Escrow is not currently held.");
        Status = "released";
    }

    public void Settle()
    {
        if (Status != "held") throw new DomainException("Escrow is not currently held.");
        Status = "settled";
    }
}
