using Contracts.Contracts;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public sealed record DeleteLeagueCommand(
    Guid LeagueId
) : IRequest<ServiceResponse<bool>>;
