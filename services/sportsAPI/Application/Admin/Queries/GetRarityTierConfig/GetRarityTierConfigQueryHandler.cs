using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Queries.GetRarityTierConfig;

public sealed class GetRarityTierConfigQueryHandler
    : IRequestHandler<GetRarityTierConfigQuery, ServiceResponse<List<RarityTierConfigDto>>>
{
    private readonly IRepository _repo;

    public GetRarityTierConfigQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<List<RarityTierConfigDto>>> Handle(
        GetRarityTierConfigQuery request,
        CancellationToken cancellationToken)
    {
        var tiers = await _repo.Query<RarityTierConfig>()
            .Where(r => r.LeagueId == request.LeagueId)
            .OrderBy(r => r.RarityName)
            .Select(r => new RarityTierConfigDto(r.Id, r.RarityName, r.RatingMin, r.RatingMax, r.PullWeightBps))
            .ToListAsync(cancellationToken);

        return ServiceResponse.Ok(tiers, $"{tiers.Count} rarity tier(s) found.");
    }
}
