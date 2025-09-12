using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public record DeleteLeagueCommand(LeagueId LeagueId) : IRequest<Result<bool>>;
