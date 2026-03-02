using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;

namespace Application.Cards.Queries.GetCollection;

public record GetCollectionQuery(Guid UserId, Guid LeagueId)
    : IRequest<ServiceResponse<List<UserCardDto>>>;
