namespace Application.Dto.Marketplace;

public class BidDto
{
    public Guid Id { get; set; }
    public Guid BidderId { get; set; }
    public long Amount { get; set; }
    public DateTime Timestamp { get; set; }
}
