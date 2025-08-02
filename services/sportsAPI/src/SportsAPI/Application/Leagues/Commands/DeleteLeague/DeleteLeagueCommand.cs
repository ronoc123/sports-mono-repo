using Application.Common.Models;
using MediatR;

namespace Application.Leagues.Commands.DeleteLeague;

public record DeleteLeagueCommand(Guid LeagueId) : IRequest<Result<bool>>;
