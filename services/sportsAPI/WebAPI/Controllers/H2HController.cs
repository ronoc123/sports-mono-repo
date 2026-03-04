using Application.H2H.Commands.CreateMatch;
using Application.H2H.Queries.GetMatchDetail;
using Application.H2H.Queries.GetMatchHistory;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/h2h/matches")]
[ApiController]
//[Authorize]
public class H2HController : ControllerBase
{
    private readonly IMediator _mediator;

    public H2HController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// POST /api/h2h/matches — Create and immediately resolve an H2H match.
    /// </summary>
    [HttpPost]
    //[Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<ServiceResponse<CreateMatchResult>>> CreateMatch(
        [FromBody] CreateMatchRequest body)
        => await _mediator.Send(new CreateMatchCommand(
            body.FanUserId,
            body.OrgId,
            body.UserCardIds,
            body.WagerAmount));

    /// <summary>
    /// GET /api/h2h/matches/{matchId} — Full match detail including bot squad and fan squad.
    /// </summary>
    [HttpGet("{matchId:guid}")]
    public async Task<ActionResult<MatchDetailDto>> GetMatchDetail(Guid matchId)
    {
        var result = await _mediator.Send(new GetMatchDetailQuery(matchId));
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// GET /api/h2h/matches?fanUserId=&amp;orgId= — Paginated match history for a fan.
    /// </summary>
    [HttpGet]
    public async Task<MatchHistoryResult> GetMatchHistory(
        [FromQuery] Guid fanUserId,
        [FromQuery] Guid orgId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => await _mediator.Send(new GetMatchHistoryQuery(fanUserId, orgId, page, pageSize));
}

public record CreateMatchRequest(Guid FanUserId, Guid OrgId, List<Guid> UserCardIds, long WagerAmount);
