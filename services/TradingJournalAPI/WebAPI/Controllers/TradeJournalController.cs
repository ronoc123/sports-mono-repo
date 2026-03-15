using Application.Dto;
using Application.TradeJournal.Commands.CreateTrade;
using Application.TradeJournal.Commands.UpsertJournalEntry;
using Application.TradeJournal.Queries.GetTrade;
using Application.TradeJournal.Queries.GetUserTrades;
using Contracts.Contracts;
using Domain.TradeJournal;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TradingJournalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TradeJournalController : ControllerBase
{
    private readonly IMediator _mediator;

    public TradeJournalController(IMediator mediator) => _mediator = mediator;

    //private Guid CurrentUserId
    //{
    //    get
    //    {
    //        var raw = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    //        if (raw is null || !Guid.TryParse(raw, out var guid))
    //            throw new UnauthorizedAccessException("User ID claim not found.");
    //        return guid;
    //    }
    //}

    private Guid CurrentUserId
    {
        get
        {
            return Guid.Parse("bb048163-62ee-4c37-a6ba-6b1b3ba74c1b");
            var raw = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (raw is null || !Guid.TryParse(raw, out var guid))
                throw new UnauthorizedAccessException("User ID claim not found.");
        }
    }

    [HttpGet]
    public async Task<ServiceResponse<List<TradeDto>>> GetMyTrades()
        => await _mediator.Send(new GetUserTradesQuery(CurrentUserId));

    [HttpGet("{id:guid}")]
    public async Task<ServiceResponse<TradeDto>> GetTrade(Guid id)
        => await _mediator.Send(new GetTradeQuery(CurrentUserId, id));

    [HttpPost]
    public async Task<ServiceResponse<TradeDto>> CreateTrade([FromBody] CreateTradeRequest request)
        => await _mediator.Send(new CreateTradeCommand(CurrentUserId, request.Symbol, request.Notes));

    [HttpPut("{tradeId:guid}/entries")]
    public async Task<ServiceResponse<JournalEntryDto>> UpsertEntry(
        Guid tradeId,
        [FromBody] UpsertJournalEntryRequest request)
        => await _mediator.Send(new UpsertJournalEntryCommand(
            CurrentUserId, tradeId, request.Phase, request.EmotionalState, request.IsEffortless, request.FreeFormNotes));
}

public record CreateTradeRequest(string Symbol, string? Notes);
public record UpsertJournalEntryRequest(
    TradePhase Phase,
    EmotionalState EmotionalState,
    bool IsEffortless,
    string? FreeFormNotes);
