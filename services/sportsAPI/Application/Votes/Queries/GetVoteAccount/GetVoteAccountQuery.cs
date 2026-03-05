using Application.Dto.VoteAccount;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Votes.Queries.GetVoteAccount
{
    public record GetVoteAccountQuery(UserId UserId, LeagueId LeagueId) : IRequest<ServiceResponse<VoteAccountDto>>;
}
