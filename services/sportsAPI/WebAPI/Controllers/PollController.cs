using Application.Poll.Commands.ArchivePoll;
using Application.Poll.Commands.CreatePoll;
using Application.Poll.Commands.SubmitPollVote;
using Application.Poll.Queries.GetPolls;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/poll")]
[ApiController]
public class PollController : ControllerBase
{
    private readonly IMediator _mediator;

    public PollController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// GET /api/poll?organizationId={orgId} — List all polls with option vote breakdowns.
    /// </summary>
    [HttpGet]
    public async Task<ServiceResponse<List<PollDto>>> GetPolls(
        [FromQuery] Guid organizationId, CancellationToken ct)
        => await _mediator.Send(new GetPollsQuery(organizationId), ct);

    /// <summary>
    /// POST /api/poll — Create a new poll (immediately Active).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ServiceResponse<CreatePollResult>> CreatePoll(
        [FromBody] CreatePollCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);

    /// <summary>
    /// PUT /api/poll/{pollId}/archive — Archive a poll, removing it from the fan dashboard.
    /// </summary>
    [HttpPut("{pollId:guid}/archive")]
    [Authorize]
    public async Task<ServiceResponse<bool>> ArchivePoll(
        Guid pollId, CancellationToken ct)
        => await _mediator.Send(new ArchivePollCommand(pollId), ct);

    /// <summary>
    /// POST /api/poll/vote — Submit a fan vote on a poll option.
    /// </summary>
    [HttpPost("vote")]
    [Authorize]
    public async Task<ServiceResponse<SubmitPollVoteResult>> SubmitPollVote(
        [FromBody] SubmitPollVoteCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);
}
