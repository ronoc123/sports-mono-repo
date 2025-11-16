using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public sealed record DeleteLeagueCommand(
    LeagueId LeagueId
) : IRequest<ServiceResponse<bool>>;
