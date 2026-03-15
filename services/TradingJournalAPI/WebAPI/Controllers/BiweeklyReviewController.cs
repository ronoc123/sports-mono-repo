using Application.BiweeklyReview.Commands.SendReviewMessage;
using Application.BiweeklyReview.Queries.GetReviewSession;
using Application.BiweeklyReview.Queries.GetReviewStatus;
using Application.Dto;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TradingJournalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BiweeklyReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public BiweeklyReviewController(IMediator mediator) => _mediator = mediator;

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

    [HttpGet("status")]
    public async Task<ServiceResponse<ReviewStatusDto>> GetStatus()
        => await _mediator.Send(new GetReviewStatusQuery(CurrentUserId));

    [HttpGet("{sessionId:guid}")]
    public async Task<ServiceResponse<ReviewSessionDto>> GetSession(Guid sessionId)
        => await _mediator.Send(new GetReviewSessionQuery(sessionId));

    [HttpPost("message")]
    public async Task<ServiceResponse<ReviewSessionDto>> SendMessage(
        [FromBody] SendReviewMessageRequest request)
        => await _mediator.Send(new SendReviewMessageCommand(
            CurrentUserId, request.ExistingSessionId, request.UserMessage));
}

public record SendReviewMessageRequest(Guid? ExistingSessionId, string UserMessage);
