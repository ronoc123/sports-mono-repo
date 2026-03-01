namespace Application.Dto.Cards;

public class PackPurchaseResultDto
{
    public Guid PackId { get; init; }
    public long PointsSpent { get; init; }
    public long RemainingBalance { get; init; }
    public IReadOnlyList<UserCardDto> Cards { get; init; } = [];
}
