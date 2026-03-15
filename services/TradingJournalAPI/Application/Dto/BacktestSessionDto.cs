namespace Application.Dto;

public class BacktestSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool IsGateUnlocked { get; set; }
    public BacktestMetricsDto? Metrics { get; set; }
    public List<IntegrityAnswerDto> Answers { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}

public class BacktestMetricsDto
{
    public int TotalTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal AvgRR { get; set; }
    public decimal Expectancy { get; set; }
}

public class IntegrityAnswerDto
{
    public Guid Id { get; set; }
    public int QuestionIndex { get; set; }
    public bool Answer { get; set; }
    public string? JournaledReason { get; set; }
}

public class BacktestStatusDto
{
    public bool IsGateUnlocked { get; set; }
    public decimal? Expectancy { get; set; }
    public int AnsweredCount { get; set; }
    public bool MetricsSubmitted { get; set; }
    public List<IntegrityAnswerDto> Answers { get; set; } = new();
}
