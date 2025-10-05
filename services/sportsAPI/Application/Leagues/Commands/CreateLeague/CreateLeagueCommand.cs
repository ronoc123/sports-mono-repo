using Application.Common.Models;
using Contracts.Contracts;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Leagues.Commands.CreateLeague;

public record CreateLeagueCommand(string Name) : IRequest<ServiceResponse<Guid>>;
