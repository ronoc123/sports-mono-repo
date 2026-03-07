using Application.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// GET /api/dashboard/{organizationId}?userId={userId}
    /// Returns trending player options, active trivia questions (with answered state), and active poll.
    /// </summary>
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetDashboard(
        Guid organizationId,
        [FromQuery] Guid userId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardQuery(organizationId, userId), ct);
        return Ok(result);
    }
}
