using MediatR;

namespace Application.H2H.Queries.GetMatchDetail;

public record GetMatchDetailQuery(Guid MatchId) : IRequest<MatchDetailDto?>;

public record MatchDetailDto(
    Guid MatchId,
    Guid FanUserId,
    long WagerAmount,
    int FanTeamOverall,
    int BotTeamOverall,
    string BotSquadSnapshot,
    string Outcome,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<SquadCardDto> FanSquadCards
);

public record SquadCardDto(
    Guid UserCardId,
    string PlayerName,
    string Position,
    int OverallRating,
    string RarityTier,
    int SlotIndex
);
