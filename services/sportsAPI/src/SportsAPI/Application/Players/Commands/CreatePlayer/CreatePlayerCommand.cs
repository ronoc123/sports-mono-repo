using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Players.Commands.CreatePlayer;

public record CreatePlayerCommand(
    string Name,
    string Position,
    string ImageUrl,
    int Age,
    LeagueId LeagueId,
    OrganizationId? OrganizationId = null
) : IRequest<Result<Guid>>;
