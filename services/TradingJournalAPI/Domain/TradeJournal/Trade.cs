using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.TradeJournal;

public sealed class Trade : Aggregate<TradeId>
{
    private readonly List<JournalEntry> _journalEntries = new();

    internal Trade() { }

    public Guid UserId { get; private set; }
    public string Symbol { get; private set; } = default!;
    public string? Notes { get; private set; }
    public IReadOnlyList<JournalEntry> JournalEntries => _journalEntries.AsReadOnly();

    public static Trade Create(Guid userId, string symbol, string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");
        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Symbol is required.");

        return new Trade
        {
            Id = TradeId.New(),
            UserId = userId,
            Symbol = symbol.Trim().ToUpperInvariant(),
            Notes = notes
        };
    }

    public JournalEntry UpsertJournalEntry(
        TradePhase phase,
        EmotionalState emotionalState,
        bool isEffortless,
        string? freeFormNotes)
    {
        var existing = _journalEntries.FirstOrDefault(e => e.Phase == phase);
        if (existing is not null)
        {
            existing.Update(emotionalState, isEffortless, freeFormNotes);
            return existing;
        }

        if (_journalEntries.Count >= 4)
            throw new DomainException("A trade can have at most 4 journal entries (one per phase).");

        var entry = JournalEntry.Create(Id, phase, emotionalState, isEffortless, freeFormNotes);
        _journalEntries.Add(entry);
        return entry;
    }
}
