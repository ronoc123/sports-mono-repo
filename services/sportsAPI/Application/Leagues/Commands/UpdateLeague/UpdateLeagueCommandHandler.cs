using Contracts.Contracts;
using Domain.Leagues;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Leagues.Commands.UpdateLeague;

public sealed class UpdateLeagueCommandHandler
  : IRequestHandler<UpdateLeagueCommand, ServiceResponse<bool>>
{
  private readonly IRepository _repo;

  public UpdateLeagueCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

  public async Task<ServiceResponse<bool>> Handle(UpdateLeagueCommand request, CancellationToken ct)
  {
    // Load
    var league = await _repo.GetByIdAsync<League, LeagueId>(request.LeagueId, ct)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    league.Rename(request.Name);

    // Persist
    _repo.Update(league);
    await _repo.SaveChangesAsync(ct);

    // Return success envelope
    return ServiceResponse.Ok(true, "League successfully updated.");
  }
}
