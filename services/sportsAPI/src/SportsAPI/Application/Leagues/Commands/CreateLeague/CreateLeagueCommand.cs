using Application.Common.Models;
using MediatR;

namespace Application.Leagues.Commands.CreateLeague;

public record CreateLeagueCommand(string Name) : IRequest<Result<Guid>>;
