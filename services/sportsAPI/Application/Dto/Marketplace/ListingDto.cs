namespace Application.Dto.Marketplace;

public class ListingDto
{
    public Guid Id { get; set; }
    public Guid CardOwnerId { get; set; }
    public Guid SellerId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int OverallRating { get; set; }
    public string RarityTier { get; set; } = string.Empty;
    public long StartingBid { get; set; }
    public long? BuyNowPrice { get; set; }
    public long CurrentBid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BidCount { get; set; }
}
