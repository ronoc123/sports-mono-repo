using Domain.Abstractions;

namespace Domain.TradeJournal;

public class JournalEntry : Entity<Guid>
{
    internal JournalEntry() { }

    public TradeId TradeId { get; private set; } = default!;
    public TradePhase Phase { get; private set; }
    public EmotionalState EmotionalState { get; private set; }
    public bool IsEffortless { get; private set; }
    public string? FreeFormNotes { get; private set; }

    // Navigation — one linked AI session per entry (optional)
    public Guid? AISessionId { get; internal set; }

    internal static JournalEntry Create(
        TradeId tradeId,
        TradePhase phase,
        EmotionalState emotionalState,
        bool isEffortless,
        string? freeFormNotes)
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            TradeId = tradeId,
            Phase = phase,
            EmotionalState = emotionalState,
            IsEffortless = isEffortless,
            FreeFormNotes = freeFormNotes
        };
    }

    internal void Update(EmotionalState emotionalState, bool isEffortless, string? freeFormNotes)
    {
        EmotionalState = emotionalState;
        IsEffortless = isEffortless;
        FreeFormNotes = freeFormNotes;
    }
}
