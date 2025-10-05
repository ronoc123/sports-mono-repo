using Application.Common.Models;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.UpdateLeague;

public sealed record UpdateLeagueCommand(
    Guid LeagueId,
    string Name
) : IRequest<ServiceResponse<bool>>;
