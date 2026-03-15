using Application.Dto;
using Contracts.Contracts;
using Domain.TradeJournal;
using MediatR;

namespace Application.TradeJournal.Queries.GetUserTrades;

public sealed class GetUserTradesQueryHandler
    : IRequestHandler<GetUserTradesQuery, ServiceResponse<List<TradeDto>>>
{
    private readonly ITradeRepository _repo;

    public GetUserTradesQueryHandler(ITradeRepository repo) => _repo = repo;

    public async Task<ServiceResponse<List<TradeDto>>> Handle(
        GetUserTradesQuery request, CancellationToken cancellationToken)
    {
        var trades = await _repo.GetByUserIdAsync(request.UserId, cancellationToken);

        var dtos = trades.Select(t => new TradeDto
        {
            Id = t.Id.Value,
            UserId = t.UserId,
            Symbol = t.Symbol,
            Notes = t.Notes,
            JournalEntries = t.JournalEntries.Select(e => new JournalEntryDto
            {
                Id = e.Id,
                Phase = e.Phase,
                EmotionalState = e.EmotionalState,
                IsEffortless = e.IsEffortless,
                FreeFormNotes = e.FreeFormNotes,
                AISessionId = e.AISessionId
            }).ToList(),
            CreatedAt = t.CreatedAt
        }).ToList();

        return ServiceResponse.Ok(dtos, "Success");
    }
}
