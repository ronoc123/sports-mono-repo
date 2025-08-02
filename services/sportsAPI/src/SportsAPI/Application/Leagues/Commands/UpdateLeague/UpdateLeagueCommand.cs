using Application.Common.Models;
using MediatR;

namespace Application.Leagues.Commands.UpdateLeague;

public record UpdateLeagueCommand(
    Guid LeagueId,
    string Name
) : IRequest<Result<bool>>;
