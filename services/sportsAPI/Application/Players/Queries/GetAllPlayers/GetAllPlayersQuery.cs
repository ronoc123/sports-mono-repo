using Application.Dto.Player;
using Contracts.Contracts;
using MediatR;

namespace Application.Players.Queries.GetAllPlayers;

public record GetAllPlayersQuery(Guid LeagueId) : IRequest<ServiceResponse<List<PlayerDto>>>;

