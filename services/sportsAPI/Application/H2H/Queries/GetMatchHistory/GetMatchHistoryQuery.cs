using MediatR;

namespace Application.H2H.Queries.GetMatchHistory;

public record GetMatchHistoryQuery(
    Guid FanUserId,
    Guid OrgId,
    int Page = 1,
    int PageSize = 20
) : IRequest<MatchHistoryResult>;

public record MatchHistoryResult(
    List<MatchSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record MatchSummaryDto(
    Guid MatchId,
    long WagerAmount,
    int FanTeamOverall,
    int BotTeamOverall,
    string Outcome,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
