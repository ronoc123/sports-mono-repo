using Domain.TradeJournal;

namespace Application.Dto;

public class TradeDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = default!;
    public string? Notes { get; set; }
    public List<JournalEntryDto> JournalEntries { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public TradePhase Phase { get; set; }
    public EmotionalState EmotionalState { get; set; }
    public bool IsEffortless { get; set; }
    public string? FreeFormNotes { get; set; }
    public Guid? AISessionId { get; set; }
}
