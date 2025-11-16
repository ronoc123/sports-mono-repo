using Contracts.Contracts;
using BuildingBlocks.Exceptions; // EntityNotFoundException, DomainException, ErrorCodes
using Domain.Repositories;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects.ConcreteTypes;
using Domain.Leagues;

namespace Application.Leagues.Commands.DeleteLeague;

public sealed class DeleteLeagueCommandHandler
  : IRequestHandler<DeleteLeagueCommand, ServiceResponse<bool>>
{
  private readonly IRepository _repo;

  public DeleteLeagueCommandHandler(IRepository repo)
  {
     _repo = repo;
  }

  public async Task<ServiceResponse<bool>> Handle(DeleteLeagueCommand request, CancellationToken ct)
  {
    // 1) Ensure the league exists
    var league = await _repo.GetByIdAsync<League, LeagueId>(request.LeagueId, ct)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    _repo.Remove<League>(league);
    await _repo.SaveChangesAsync(ct);

    // 4) Success envelope
    return ServiceResponse.Ok(true, "League successfully deleted.");
  }
}
