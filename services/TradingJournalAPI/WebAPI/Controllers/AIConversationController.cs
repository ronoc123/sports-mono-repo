using Application.AIConversation.Commands.SendAIMessage;
using Application.AIConversation.Queries.GetAISession;
using Application.Dto;
using Contracts.Contracts;
using Domain.AIConversation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TradingJournalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AIConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIConversationController(IMediator mediator) => _mediator = mediator;

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

    [HttpGet("by-entity/{linkedEntityId:guid}")]
    public async Task<ServiceResponse<AISessionDto?>> GetByEntity(Guid linkedEntityId)
        => await _mediator.Send(new GetAISessionQuery(linkedEntityId));

    [HttpPost("message")]
    public async Task<ServiceResponse<AISessionDto>> SendMessage([FromBody] SendAIMessageRequest request)
        => await _mediator.Send(new SendAIMessageCommand(
            CurrentUserId,
            request.ExistingSessionId,
            request.Context,
            request.LinkedEntityId,
            request.UserMessage,
            request.PhaseHint,
            request.EmotionalStateHint,
            request.IsEffortlessHint,
            request.NotesHint));
}

public record SendAIMessageRequest(
    Guid? ExistingSessionId,
    AISessionContext Context,
    Guid LinkedEntityId,
    string UserMessage,
    string? PhaseHint,
    string? EmotionalStateHint,
    bool? IsEffortlessHint,
    string? NotesHint);
