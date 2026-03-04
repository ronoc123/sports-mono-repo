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

        // Load UserCards with CardPlayer navigation
        var userCardIds = squadCards.Select(sc => sc.UserCardId).ToList();
        var userCards = await _repo.Query<UserCard>(asNoTracking: true)
            .Include(uc => uc.CardPlayer)
            .Where(uc => userCardIds.Contains(uc.Id))
            .ToDictionaryAsync(uc => uc.Id, cancellationToken);

        var squadCardDtos = squadCards
            .Where(sc => userCards.ContainsKey(sc.UserCardId))
            .Select(sc =>
            {
                var uc = userCards[sc.UserCardId];
                var cp = uc.CardPlayer!;
                return new SquadCardDto(
                    uc.Id,
                    cp.Name,
                    cp.Position,
                    cp.OverallRating,
                    uc.RarityTier,
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
