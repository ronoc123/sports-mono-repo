using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.TradeJournal;
using MediatR;

namespace Application.TradeJournal.Commands.UpsertJournalEntry;

public sealed class UpsertJournalEntryCommandHandler
    : IRequestHandler<UpsertJournalEntryCommand, ServiceResponse<JournalEntryDto>>
{
    private readonly ITradeRepository _repo;

    public UpsertJournalEntryCommandHandler(ITradeRepository repo) => _repo = repo;

    public async Task<ServiceResponse<JournalEntryDto>> Handle(
        UpsertJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var trade = await _repo.GetByIdWithEntriesAsync(TradeId.From(request.TradeId), cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Trade), request.TradeId);

        if (trade.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this trade.");

        var isNew = !trade.JournalEntries.Any(e => e.Phase == request.Phase);

        var entry = trade.UpsertJournalEntry(
            request.Phase,
            request.EmotionalState,
            request.IsEffortless,
            request.FreeFormNotes);

        if (isNew)
            _repo.AddEntry(entry);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(new JournalEntryDto
        {
            Id = entry.Id,
            Phase = entry.Phase,
            EmotionalState = entry.EmotionalState,
            IsEffortless = entry.IsEffortless,
            FreeFormNotes = entry.FreeFormNotes,
            AISessionId = entry.AISessionId
        }, "Journal entry saved.");
    }
}
