using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.BacktestIntegrity;

public sealed class BacktestSession : Aggregate<BacktestSessionId>
{
    private readonly List<IntegrityAnswer> _answers = new();

    internal BacktestSession() { }

    public Guid UserId { get; private set; }
    public BacktestMetrics? Metrics { get; private set; }
    public IReadOnlyList<IntegrityAnswer> Answers => _answers.AsReadOnly();

    /// <summary>
    /// Gate is unlocked when all 6 answers are true AND expectancy > 0.
    /// </summary>
    public bool IsGateUnlocked =>
        Metrics is not null &&
        Metrics.Expectancy > 0 &&
        _answers.Count == 6 &&
        _answers.All(a => a.Answer);

    public static BacktestSession Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");

        return new BacktestSession
        {
            Id = BacktestSessionId.New(),
            UserId = userId
        };
    }

    public void SubmitMetrics(int totalTrades, decimal winRate, decimal avgRR)
    {
        if (totalTrades <= 0)
            throw new DomainException("TotalTrades must be greater than 0.");
        if (winRate < 0 || winRate > 1)
            throw new DomainException("WinRate must be between 0 and 1.");
        if (avgRR <= 0)
            throw new DomainException("AvgRR must be greater than 0.");

        Metrics = BacktestMetrics.Compute(totalTrades, winRate, avgRR);
    }

    public void SubmitAnswers(IReadOnlyList<(int QuestionIndex, bool Answer, string? Reason)> answers)
    {
        if (answers.Count != 6)
            throw new DomainException("Exactly 6 integrity answers must be submitted.");

        var indices = answers.Select(a => a.QuestionIndex).OrderBy(i => i).ToList();
        if (!indices.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            throw new DomainException("Question indices must be 1 through 6.");

        _answers.Clear();
        foreach (var (index, answer, reason) in answers)
        {
            _answers.Add(IntegrityAnswer.Create(Id, index, answer, reason));
        }
    }
}
