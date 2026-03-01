using Domain.Cards;
using Domain.H2H;
using Domain.H2H.Services;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.H2H.Commands.ResolveMatch;

public sealed class ResolveMatchCommandHandler : IRequestHandler<ResolveMatchCommand>
{
    private readonly IRepository _repo;
    private readonly ILogger<ResolveMatchCommandHandler> _logger;

    public ResolveMatchCommandHandler(
        IRepository repo,
        ILogger<ResolveMatchCommandHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Handle(ResolveMatchCommand request, CancellationToken cancellationToken)
    {
        // 1. Load match (tracked — will be mutated)
        var match = await _repo.Query<H2HMatch>(asNoTracking: false)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match is null)
        {
            _logger.LogWarning("ResolveMatchCommand: match {MatchId} not found.", request.MatchId);
            return;
        }

        // 2. Idempotent guard
        if (match.Status == "completed")
        {
            _logger.LogDebug("ResolveMatchCommand: match {MatchId} already completed. No-op.", request.MatchId);
            return;
        }

        // 3. Build bot squad from real CardPlayers in the fan's league
        //    Weighted toward fan's overall ±15; fallback to any card if not enough candidates.
        const int BotSquadSize = 5;
        const int OvrWindow = 15;

        var allLeagueCards = await _repo.Query<CardPlayer>(asNoTracking: true)
            .Where(cp => cp.LeagueId == match.LeagueId)
            .ToListAsync(cancellationToken);

        var preferredPool = allLeagueCards
            .Where(cp => Math.Abs(cp.OverallRating - match.FanTeamOverall) <= OvrWindow)
            .ToList();

        var pool = preferredPool.Count >= BotSquadSize ? preferredPool : allLeagueCards;

        // Shuffle and take up to 5
        var rng = new Random();
        var botSquad = pool
            .OrderBy(_ => rng.Next())
            .Take(BotSquadSize)
            .ToList();

        int botTeamOverall = botSquad.Count > 0
            ? (int)Math.Round(botSquad.Average(cp => cp.OverallRating))
            : match.FanTeamOverall; // edge case: no cards in league

        // 4. Determine outcome
        bool fanWins = BotResolutionEngine.FanWins(match.FanTeamOverall, botTeamOverall, rng);
        string outcome = fanWins ? "win" : "loss";

        // 5. Serialize bot squad snapshot
        var snapshotData = botSquad.Select(cp => new
        {
            name = cp.Name,
            overallRating = cp.OverallRating,
            rarityTier = cp.RarityTier,
        });
        string botSquadSnapshot = JsonSerializer.Serialize(snapshotData);

        // 6. Load fan VoteAccount (tracked)
        var fanAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(match.OrgId),
            UserId.Of(match.FanUserId));

        if (fanAccount is null)
        {
            _logger.LogError(
                "ResolveMatchCommand: fan VoteAccount not found for match {MatchId}. Aborting resolution.",
                request.MatchId);
            return;
        }

        // 7. Credit winnings if fan wins (wager × 2 returns net +wager)
        if (fanWins)
        {
            fanAccount.CreditH2HWinnings(match.WagerAmount * 2, match.Id.ToString());
            _repo.Update(fanAccount);
        }

        // 8. Complete the match record
        match.Complete(outcome, botTeamOverall, botSquadSnapshot);
        _repo.Update(match);

        // 9. Persist — single SaveChanges = implicit transaction
        await _repo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ResolveMatchCommand: match {MatchId} resolved. Outcome={Outcome}, FanOvr={FanOvr}, BotOvr={BotOvr}.",
            request.MatchId, outcome, match.FanTeamOverall, botTeamOverall);
    }
}
