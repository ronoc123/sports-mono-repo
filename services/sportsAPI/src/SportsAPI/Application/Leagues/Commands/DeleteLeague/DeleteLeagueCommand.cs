using Contracts.Contracts;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public sealed record DeleteLeagueCommand(
    Domain.ValueObjects.ConcreteTypes.LeagueId LeagueId
) : IRequest<ServiceResponse<bool>>;
