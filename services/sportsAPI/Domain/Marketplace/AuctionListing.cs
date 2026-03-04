using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Marketplace;

/// <summary>
/// Represents an active or settled auction listing for a fan's card.
/// Status lifecycle: active → sold | settled | expired | cancelled
/// </summary>
public sealed class AuctionListing : Aggregate<Guid>
{
    internal AuctionListing() { } // EF

    public Guid UserCardId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid LeagueId { get; private set; }

    public long StartingBid { get; private set; }
    public long? BuyNowPrice { get; private set; }
    public long CurrentBid { get; private set; }

    public string Status { get; private set; } = "active";
    public DateTime ExpiresAt { get; private set; }

    public static readonly int[] ValidDurationHours = [1, 24, 48, 72];

    public static AuctionListing Create(
        Guid userCardId,
        Guid sellerId,
        Guid leagueId,
        long startingBid,
        long? buyNowPrice,
        int durationHours)
    {
        if (userCardId == Guid.Empty) throw new DomainException("UserCardId is required.");
        if (sellerId == Guid.Empty) throw new DomainException("SellerId is required.");
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");
        if (startingBid <= 0) throw new DomainException("Starting bid must be greater than zero.");
        if (!ValidDurationHours.Contains(durationHours))
            throw new DomainException($"Duration must be one of: {string.Join(", ", ValidDurationHours)} hours.");
        if (buyNowPrice.HasValue && buyNowPrice.Value <= startingBid)
            throw new DomainException("Buy now price must be greater than starting bid.");

        return new AuctionListing
        {
            Id = Guid.NewGuid(),
            UserCardId = userCardId,
            SellerId = sellerId,
            LeagueId = leagueId,
            StartingBid = startingBid,
            BuyNowPrice = buyNowPrice,
            CurrentBid = startingBid,
            Status = "active",
            ExpiresAt = DateTime.UtcNow.AddHours(durationHours),
        };
    }

    public void UpdateCurrentBid(long newBid)
    {
        if (Status != "active") throw new DomainException("Cannot bid on an inactive listing.");
        if (newBid <= CurrentBid) throw new DomainException("New bid must exceed current bid.");
        CurrentBid = newBid;
    }

    public void MarkAsSold() => Status = "sold";
    public void MarkAsSettled() => Status = "settled";
    public void MarkAsExpired() => Status = "expired";
    public void Cancel() => Status = "cancelled";

    public bool IsActive => Status == "active" && ExpiresAt > DateTime.UtcNow;
}
