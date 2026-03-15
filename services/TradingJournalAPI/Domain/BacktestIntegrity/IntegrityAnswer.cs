using Domain.Abstractions;

namespace Domain.BacktestIntegrity;

public class IntegrityAnswer : Entity<Guid>
{
    internal IntegrityAnswer() { }

    public BacktestSessionId SessionId { get; private set; } = default!;
    public int QuestionIndex { get; private set; }   // 1-6
    public bool Answer { get; private set; }
    public string? JournaledReason { get; private set; }

    internal static IntegrityAnswer Create(BacktestSessionId sessionId, int questionIndex, bool answer, string? reason)
    {
        return new IntegrityAnswer
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            QuestionIndex = questionIndex,
            Answer = answer,
            JournaledReason = reason
        };
    }
}
