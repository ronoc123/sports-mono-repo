using Domain.BiweeklyReview;

namespace Application.Dto;

public class ReviewSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<ReviewMessageDto> Messages { get; set; } = new();
}

public class ReviewMessageDto
{
    public Guid Id { get; set; }
    public ReviewMessageRole Role { get; set; }
    public string Content { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}

public class ReviewStatusDto
{
    public bool IsReady { get; set; }
    public int TradeCount { get; set; }
    public int TradesRequired { get; set; }
    public int DaysUntilNext { get; set; }
    public Guid? SessionId { get; set; }
}
