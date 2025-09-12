using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.UpdateLeague;

public record UpdateLeagueCommand(
    LeagueId LeagueId,
    string Name
) : IRequest<Result<bool>>;
