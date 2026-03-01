using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;

namespace Application.Cards.Commands.CreateCardPlayer;

public record CreateCardPlayerCommand(
    Guid LeagueId,
    Guid OrgId,
    string Name,
    string Position,
    int OverallRating
) : IRequest<ServiceResponse<CardPlayerDto>>;
