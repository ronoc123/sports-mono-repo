namespace Application.Dto.Cards;

public class CardPlayerDto
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public string Name { get; set; } = default!;
    public string Position { get; set; } = default!;
    public int OverallRating { get; set; }
    public string RarityTier { get; set; } = default!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
