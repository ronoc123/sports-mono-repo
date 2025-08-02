using Application.Common.Models;
using MediatR;

namespace Application.Players.Commands.CreatePlayer;

public record CreatePlayerCommand(
    string Name,
    string Position,
    string ImageUrl,
    int Age,
    Guid LeagueId,
    Guid? OrganizationId = null
) : IRequest<Result<Guid>>;
