using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;

namespace Application.Cards.Commands.UpdateCardPlayer;

public record UpdateCardPlayerCommand(
    Guid CardPlayerId,
    Guid OrgId,       // used to resolve rarity and validate GM ownership
    Guid LeagueId,    // requesting GM's leagueId — must match card's leagueId
    string Name,
    string Position,
    int OverallRating
) : IRequest<ServiceResponse<CardPlayerDto>>;
