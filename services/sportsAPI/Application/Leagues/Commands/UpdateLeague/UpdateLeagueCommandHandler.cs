using Contracts.Contracts;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Leagues.Commands.UpdateLeague;

public sealed class UpdateLeagueCommandHandler
  : IRequestHandler<UpdateLeagueCommand, ServiceResponse<bool>>
{
  private readonly ILeagueRepository _leagues;

  public UpdateLeagueCommandHandler(ILeagueRepository leagues)
  {
    _leagues = leagues;
  }

  public async Task<ServiceResponse<bool>> Handle(UpdateLeagueCommand request, CancellationToken ct)
  {
    // Load
    var league = await _leagues.GetByIdAsync(LeagueId.Of(request.LeagueId), ct)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    league.SetName(request.Name); 

    // Persist
    _leagues.Update(league);
    await _leagues.SaveChangesAsync(ct);

    // Return success envelope
    return ServiceResponse.Ok(true, "League successfully updated.");
  }
}
