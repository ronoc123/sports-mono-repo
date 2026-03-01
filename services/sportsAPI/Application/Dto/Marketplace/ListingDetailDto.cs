namespace Application.Dto.Marketplace;

public class ListingDetailDto : ListingDto
{
    public List<BidDto> BidHistory { get; set; } = [];
}
