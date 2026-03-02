using Application.Admin.Commands.UpdatePackConfig;
using Application.Admin.Commands.UpdateRarityTierConfig;
using Application.Admin.Queries.ExportTransactionsCsv;
using Application.Admin.Queries.GetPackConfig;
using Application.Admin.Queries.GetRarityTierConfig;
using Application.Admin.Queries.GetTransactionAudit;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/admin/economy")]
[ApiController]
//[Authorize(Policy = "GMOnly")]
public class AdminEconomyController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminEconomyController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// GET /api/admin/economy/rarity-tiers/{leagueId} — List all rarity tier configs for a league.
    /// </summary>
    [HttpGet("rarity-tiers/{leagueId:guid}")]
    public async Task<ServiceResponse<List<RarityTierConfigDto>>> GetRarityTiers(Guid leagueId)
        => await _mediator.Send(new GetRarityTierConfigQuery(leagueId));

    /// <summary>
    /// PUT /api/admin/economy/rarity-tiers/{configId} — Update a rarity tier's rating range and pull weight.
    /// </summary>
    [HttpPut("rarity-tiers/{configId:guid}")]
    public async Task<ServiceResponse<RarityTierConfigDto>> UpdateRarityTier(
        Guid configId,
        [FromBody] UpdateRarityTierBody body)
        => await _mediator.Send(new UpdateRarityTierConfigCommand(configId, body.RatingMin, body.RatingMax, body.PullWeightBps));

    /// <summary>
    /// GET /api/admin/economy/pack-config/{orgId} — Get the current pack point cost for an org.
    /// </summary>
    [HttpGet("pack-config/{orgId:guid}")]
    public async Task<ServiceResponse<PackConfigDto>> GetPackConfig(Guid orgId)
        => await _mediator.Send(new GetPackConfigQuery(orgId));

    /// <summary>
    /// PUT /api/admin/economy/pack-config/{orgId} — Set the pack point cost for an org.
    /// </summary>
    [HttpPut("pack-config/{orgId:guid}")]
    public async Task<ServiceResponse<PackConfigDto>> UpdatePackConfig(
        Guid orgId,
        [FromBody] UpdatePackConfigBody body)
        => await _mediator.Send(new UpdatePackConfigCommand(orgId, body.PackPointCost));

    /// <summary>
    /// GET /api/admin/economy/transactions/{orgId} — Paginated transaction audit log.
    /// </summary>
    [HttpGet("transactions/{orgId:guid}")]
    public async Task<ServiceResponse<TransactionAuditResult>> GetTransactions(
        Guid orgId,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? reason = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => await _mediator.Send(new GetTransactionAuditQuery(orgId, userId, reason, page, pageSize));

    /// <summary>
    /// GET /api/admin/economy/transactions/{orgId}/export — Download transactions as CSV.
    /// </summary>
    [HttpGet("transactions/{orgId:guid}/export")]
    public async Task<IActionResult> ExportTransactions(
        Guid orgId,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? reason = null)
    {
        var csv = await _mediator.Send(new ExportTransactionsCsvQuery(orgId, userId, reason));
        return File(csv, "text/csv", $"transactions-{orgId}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

public record UpdateRarityTierBody(int RatingMin, int RatingMax, int PullWeightBps);
public record UpdatePackConfigBody(long PackPointCost);
