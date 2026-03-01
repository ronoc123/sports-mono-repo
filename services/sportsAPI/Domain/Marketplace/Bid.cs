using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Marketplace;

/// <summary>
/// A single bid placed by a fan on an AuctionListing.
/// Immutable once created.
/// </summary>
public sealed class Bid : Aggregate<Guid>
{
    internal Bid() { } // EF

    public Guid ListingId { get; private set; }
    public Guid BidderId { get; private set; }
    public long Amount { get; private set; }
    public DateTime Timestamp { get; private set; }

    public static Bid Create(Guid listingId, Guid bidderId, long amount)
    {
        if (listingId == Guid.Empty) throw new DomainException("ListingId is required.");
        if (bidderId == Guid.Empty) throw new DomainException("BidderId is required.");
        if (amount <= 0) throw new DomainException("Bid amount must be greater than zero.");

        return new Bid
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            BidderId = bidderId,
            Amount = amount,
            Timestamp = DateTime.UtcNow,
        };
    }
}
