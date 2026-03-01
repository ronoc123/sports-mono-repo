using Domain.Cards;
using Domain.H2H;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.H2H.Queries.GetMatchDetail;

public sealed class GetMatchDetailQueryHandler : IRequestHandler<GetMatchDetailQuery, MatchDetailDto?>
{
    private readonly IRepository _repo;

    public GetMatchDetailQueryHandler(IRepository repo) => _repo = repo;

    public async Task<MatchDetailDto?> Handle(
        GetMatchDetailQuery request,
        CancellationToken cancellationToken)
    {
        var match = await _repo.Query<H2HMatch>(asNoTracking: true)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match is null) return null;

        // Load squad cards for this match
        var squadCards = await _repo.Query<H2HSquadCard>(asNoTracking: true)
            .Where(sc => sc.MatchId == request.MatchId)
            .OrderBy(sc => sc.SlotIndex)
            .ToListAsync(cancellationToken);

        // Load CardOwners with CardPlayer navigation
        var cardOwnerIds = squadCards.Select(sc => sc.CardOwnerId).ToList();
        var cardOwners = await _repo.Query<CardOwner>(asNoTracking: true)
            .Include(co => co.CardPlayer)
            .Where(co => cardOwnerIds.Contains(co.Id))
            .ToDictionaryAsync(co => co.Id, cancellationToken);

        var squadCardDtos = squadCards
            .Where(sc => cardOwners.ContainsKey(sc.CardOwnerId))
            .Select(sc =>
            {
                var co = cardOwners[sc.CardOwnerId];
                var cp = co.CardPlayer!;
                return new SquadCardDto(
                    co.Id,
                    cp.Name,
                    cp.Position,
                    cp.OverallRating,
                    cp.RarityTier,
                    sc.SlotIndex);
            })
            .ToList();

        return new MatchDetailDto(
            match.Id,
            match.FanUserId,
            match.WagerAmount,
            match.FanTeamOverall,
            match.BotTeamOverall,
            match.BotSquadSnapshot,
            match.Outcome,
            match.Status,
            match.CreatedAt,
            match.CompletedAt,
            squadCardDtos);
    }
}
