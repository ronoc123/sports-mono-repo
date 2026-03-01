using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.H2H;

/// <summary>
/// Represents a Head-to-Head match between a fan and a bot opponent.
/// Status lifecycle: pending → completed
/// </summary>
public sealed class H2HMatch : Aggregate<Guid>
{
    internal H2HMatch() { } // EF

    public Guid FanUserId { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid LeagueId { get; private set; }
    public long WagerAmount { get; private set; }
    public int FanTeamOverall { get; private set; }
    public int BotTeamOverall { get; private set; }

    /// <summary>JSON array of bot squad card names + ratings.</summary>
    public string BotSquadSnapshot { get; private set; } = "[]";

    /// <summary>"win" | "loss" | "pending"</summary>
    public string Outcome { get; private set; } = "pending";

    /// <summary>"pending" | "completed"</summary>
    public string Status { get; private set; } = "pending";

    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public static H2HMatch Create(
        Guid fanUserId,
        Guid orgId,
        Guid leagueId,
        long wagerAmount,
        int fanTeamOverall)
    {
        if (fanUserId == Guid.Empty) throw new DomainException("FanUserId is required.");
        if (orgId == Guid.Empty) throw new DomainException("OrgId is required.");
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");
        if (wagerAmount <= 0) throw new DomainException("Wager amount must be greater than zero.");
        if (fanTeamOverall <= 0) throw new DomainException("Fan team overall must be positive.");

        return new H2HMatch
        {
            Id = Guid.NewGuid(),
            FanUserId = fanUserId,
            OrgId = orgId,
            LeagueId = leagueId,
            WagerAmount = wagerAmount,
            FanTeamOverall = fanTeamOverall,
            Status = "pending",
            Outcome = "pending",
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Complete the match with resolved outcome. Idempotent — no-op if already completed.
    /// </summary>
    public void Complete(string outcome, int botTeamOverall, string botSquadSnapshot)
    {
        if (Status == "completed") return;

        Outcome = outcome;
        BotTeamOverall = botTeamOverall;
        BotSquadSnapshot = botSquadSnapshot;
        Status = "completed";
        CompletedAt = DateTime.UtcNow;
    }
}
