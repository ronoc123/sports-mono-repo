using Application.Dto;
using Contracts.Contracts;
using Domain.TradeJournal;
using MediatR;

namespace Application.TradeJournal.Commands.UpsertJournalEntry;

public record UpsertJournalEntryCommand(
    Guid UserId,
    Guid TradeId,
    TradePhase Phase,
    EmotionalState EmotionalState,
    bool IsEffortless,
    string? FreeFormNotes
) : IRequest<ServiceResponse<JournalEntryDto>>;
