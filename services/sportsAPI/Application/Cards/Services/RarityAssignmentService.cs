using BuildingBlocks.Exceptions;
using Domain.Cards;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Cards.Services;

/// <summary>
/// Resolves the correct rarity tier name for a given overall rating by querying
/// the RarityTierConfig table. Rarity is always auto-assigned — never set manually.
/// </summary>
public class RarityAssignmentService
{
    private readonly IRepository _repo;

    public RarityAssignmentService(IRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Returns the RarityName whose [RatingMin, RatingMax] range contains <paramref name="overallRating"/>
    /// for the given league. Throws DomainException if no matching tier is configured.
    /// </summary>
    public async Task<string> AssignAsync(Guid leagueId, int overallRating, CancellationToken ct = default)
    {
        var tier = await _repo
            .Query<RarityTierConfig>()
            .Where(t => t.LeagueId == leagueId
                     && t.RatingMin <= overallRating
                     && t.RatingMax >= overallRating)
            .FirstOrDefaultAsync(ct);

        if (tier is null)
            throw new DomainException(
                $"No rarity tier configured for overall rating {overallRating} in league {leagueId}. " +
                "Configure RarityTierConfig entries first.");

        return tier.RarityName;
    }
}
