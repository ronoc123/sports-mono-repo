using BuildingBlocks.Exceptions;
using Domain.Abstractions;

namespace Domain.Cards;

/// <summary>
/// Per-organisation configuration for card pack purchases.
/// Scoped to one org — each org can set its own pack point cost.
/// </summary>
public sealed class PackConfig : Aggregate<Guid>
{
    internal PackConfig() { } // EF

    public Guid OrgId { get; private set; }

    /// <summary>Points deducted when a fan purchases one card pack.</summary>
    public long PackPointCost { get; private set; }

    public static PackConfig Create(Guid orgId, long packPointCost)
    {
        if (orgId == Guid.Empty) throw new DomainException("OrgId is required.");
        if (packPointCost <= 0) throw new DomainException("Pack point cost must be positive.");

        return new PackConfig
        {
            Id = Guid.NewGuid(),
            OrgId = orgId,
            PackPointCost = packPointCost,
        };
    }

    public void UpdateCost(long packPointCost)
    {
        if (packPointCost <= 0) throw new DomainException("Pack point cost must be positive.");
        PackPointCost = packPointCost;
    }
}
