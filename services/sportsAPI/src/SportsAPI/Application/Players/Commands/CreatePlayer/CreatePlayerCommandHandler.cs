using Application.Common.Interfaces;
using Application.Players.Queries.GetAllPlayers;
using Contracts.Contracts;
using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Players.Commands.CreatePlayer;

public sealed class CreatePlayerCommandHandler
  : IRequestHandler<CreatePlayerCommand, ServiceResponse<PlayerDto>>
{
  private readonly ILeagueRepository _leagueRepository;

  public CreatePlayerCommandHandler(
      ILeagueRepository leagueRepository,
      IApplicationDbContext context)
  {
    _leagueRepository = leagueRepository;
  }

  public async Task<ServiceResponse<PlayerDto>> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
  {

    var league = await _leagueRepository.GetByIdAsync(request.LeagueId, cancellationToken);
    if (league is null)
      throw new ValidationException("League Not Found");

    var player = League.CreatePlayer(
        request.Name,
        request.Position,
        request.ImageUrl,
        request.Age,
        request.LeagueId,
        request.OrganizationId.HasValue
            ? OrganizationId.Of(request.OrganizationId.Value)
            : null);


    league.AddPlayer(player);


    await _leagueRepository.SaveChangesAsync(cancellationToken);

    return ServiceResponse.Ok(new PlayerDto
    {
      Id = player.Id.Value,
      Name = player.Name,
      Position = player.Position,
      ImageUrl = player.ImageUrl,
      UpdatedAt =  player.UpdatedAt,
      Age = player.Age,
    }, "Player successfully created.");
  }
}


