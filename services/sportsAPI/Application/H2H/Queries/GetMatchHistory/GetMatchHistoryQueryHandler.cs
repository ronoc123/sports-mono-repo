using Domain.H2H;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.H2H.Queries.GetMatchHistory;

public sealed class GetMatchHistoryQueryHandler : IRequestHandler<GetMatchHistoryQuery, MatchHistoryResult>
{
    private readonly IRepository _repo;

    public GetMatchHistoryQueryHandler(IRepository repo) => _repo = repo;

    public async Task<MatchHistoryResult> Handle(
        GetMatchHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = _repo.Query<H2HMatch>(asNoTracking: true)
            .Where(m => m.FanUserId == request.FanUserId && m.OrgId == request.OrgId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var matches = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = matches.Select(m => new MatchSummaryDto(
            m.Id,
            m.WagerAmount,
            m.FanTeamOverall,
            m.BotTeamOverall,
            m.Outcome,
            m.Status,
            m.CreatedAt,
            m.CompletedAt)).ToList();

        return new MatchHistoryResult(items, totalCount, request.Page, request.PageSize);
    }
}
