using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.TradeJournal;
using MediatR;

namespace Application.TradeJournal.Queries.GetTrade;

public sealed class GetTradeQueryHandler : IRequestHandler<GetTradeQuery, ServiceResponse<TradeDto>>
{
    private readonly ITradeRepository _repo;

    public GetTradeQueryHandler(ITradeRepository repo) => _repo = repo;

    public async Task<ServiceResponse<TradeDto>> Handle(GetTradeQuery request, CancellationToken cancellationToken)
    {
        var trade = await _repo.GetByIdWithEntriesAsync(TradeId.From(request.TradeId), cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Trade), request.TradeId);

        if (trade.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not own this trade.");

        return ServiceResponse.Ok(new TradeDto
        {
            Id = trade.Id.Value,
            UserId = trade.UserId,
            Symbol = trade.Symbol,
            Notes = trade.Notes,
            JournalEntries = trade.JournalEntries.Select(e => new JournalEntryDto
            {
                Id = e.Id,
                Phase = e.Phase,
                EmotionalState = e.EmotionalState,
                IsEffortless = e.IsEffortless,
                FreeFormNotes = e.FreeFormNotes,
                AISessionId = e.AISessionId
            }).ToList(),
            CreatedAt = trade.CreatedAt
        }, "Success");
    }
}
