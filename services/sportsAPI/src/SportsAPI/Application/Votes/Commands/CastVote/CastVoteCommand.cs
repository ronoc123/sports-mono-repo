using Application.Common.Models;
using MediatR;

namespace Application.Votes.Commands.CastVote;

public record CastVoteCommand(
    Guid UserId,
    Guid PlayerOptionId,
    Guid OrganizationId,
    int VotesSpent
) : IRequest<Result<Guid>>;
