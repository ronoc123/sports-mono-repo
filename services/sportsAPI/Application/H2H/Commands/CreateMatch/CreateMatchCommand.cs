using Contracts.Contracts;
using MediatR;

namespace Application.H2H.Commands.CreateMatch;

public record CreateMatchCommand(
    Guid FanUserId,
    Guid OrgId,
    IReadOnlyList<Guid> CardOwnerIds,
    long WagerAmount
) : IRequest<ServiceResponse<CreateMatchResult>>;

public record CreateMatchResult(
    Guid MatchId,
    string Outcome,
    int FanTeamOverall,
    int BotTeamOverall,
    string BotSquadSnapshot,
    long UpdatedBalance,
    long WagerAmount
);
