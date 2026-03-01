using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;

namespace Application.Cards.Queries.GetCardPlayers;

public record GetCardPlayersQuery(Guid LeagueId) : IRequest<ServiceResponse<List<CardPlayerDto>>>;
