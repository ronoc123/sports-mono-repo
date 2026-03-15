using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.BacktestIntegrity;
using Domain.TradeJournal;
using MediatR;

namespace Application.TradeJournal.Commands.CreateTrade;

public sealed class CreateTradeCommandHandler
    : IRequestHandler<CreateTradeCommand, ServiceResponse<TradeDto>>
{
    private readonly ITradeRepository _tradeRepo;
    private readonly IBacktestSessionRepository _backtestRepo;

    public CreateTradeCommandHandler(ITradeRepository tradeRepo, IBacktestSessionRepository backtestRepo)
    {
        _tradeRepo = tradeRepo;
        _backtestRepo = backtestRepo;
    }

    public async Task<ServiceResponse<TradeDto>> Handle(
        CreateTradeCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _backtestRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        if (session is null || !session.IsGateUnlocked)
            throw new DomainException("Backtest integrity gate must be unlocked before creating trades.");

        var trade = Trade.Create(request.UserId, request.Symbol, request.Notes);
        await _tradeRepo.AddAsync(trade, cancellationToken);
        await _tradeRepo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(ToDto(trade), "Trade created.");
    }

    private static TradeDto ToDto(Trade t) => new()
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
    };
}
