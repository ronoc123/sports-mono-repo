using Domain.AIConversation;
using Domain.TradeJournal;

namespace Application.Common.Interfaces;

public record AIJournalContext(
    TradePhase Phase,
    EmotionalState EmotionalState,
    bool IsEffortless,
    string? Notes,
    string? RecentPatternSummary);

public record AIIntegrityContext(
    int QuestionIndex,
    string QuestionText,
    bool Answer,
    string? Reason);

public record AIReviewContext(
    string TradesSummary,
    string TopPatterns);

public interface IAIService
{
    Task<string> GenerateJournalResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIJournalContext context,
        CancellationToken ct = default);

    Task<string> GenerateIntegrityResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIIntegrityContext context,
        CancellationToken ct = default);

    Task<string> GenerateReviewResponseAsync(
        IReadOnlyList<AIMessage> history,
        AIReviewContext context,
        CancellationToken ct = default);
}
