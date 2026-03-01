namespace Application.Dto.Cards;

public class UserCardDto
{
    public Guid Id { get; init; }
    public Guid CardPackId { get; init; }
    public Guid CardPlayerId { get; init; }
    public Guid LeagueId { get; init; }
    public string Name { get; init; } = default!;
    public string Position { get; init; } = default!;
    public int OverallRating { get; init; }
    public string RarityTier { get; init; } = default!;
    public bool IsListed { get; init; }
    public DateTime? PulledAt { get; init; }
}
