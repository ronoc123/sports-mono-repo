using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Cards;

/// <summary>
/// Represents a purchased card pack. Acts as the header record
/// that groups the 5 UserCards drawn in a single pull.
/// </summary>
public sealed class CardPack : Aggregate<Guid>
{
    internal CardPack() { } // EF

    public Guid UserId { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid LeagueId { get; private set; }
    public long PointCost { get; private set; }
    public string PullSeed { get; private set; } = default!;

    public static CardPack Create(
        Guid userId,
        Guid orgId,
        Guid leagueId,
        long pointCost,
        string pullSeed)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (orgId == Guid.Empty) throw new DomainException("OrgId is required.");
        if (leagueId == Guid.Empty) throw new DomainException("LeagueId is required.");
        if (pointCost <= 0) throw new DomainException("Point cost must be positive.");
        ArgumentException.ThrowIfNullOrWhiteSpace(pullSeed);

        return new CardPack
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrgId = orgId,
            LeagueId = leagueId,
            PointCost = pointCost,
            PullSeed = pullSeed,
        };
    }
}
