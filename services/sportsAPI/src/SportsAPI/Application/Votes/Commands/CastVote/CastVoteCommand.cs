using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Votes.Commands.CastVote;

public record CastVoteCommand(
    UserId UserId,
    PlayerOptionId PlayerOptionId,
    OrganizationId OrganizationId,
    int VotesSpent
) : IRequest<Result<Guid>>;
