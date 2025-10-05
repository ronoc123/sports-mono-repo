using Contracts.Contracts;
using BuildingBlocks.Exceptions; // EntityNotFoundException, DomainException, ErrorCodes
using Domain.Repositories;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects.ConcreteTypes;

namespace Application.Leagues.Commands.DeleteLeague;

public sealed class DeleteLeagueCommandHandler
  : IRequestHandler<DeleteLeagueCommand, ServiceResponse<bool>>
{
  private readonly ILeagueRepository _leagues;
  private readonly IOrganizationRepository _orgs;

  public DeleteLeagueCommandHandler(ILeagueRepository leagues, IOrganizationRepository orgs)
  {
    _leagues = leagues;
    _orgs = orgs;
  }

  public async Task<ServiceResponse<bool>> Handle(DeleteLeagueCommand request, CancellationToken ct)
  {
    // 1) Ensure the league exists
    var league = await _leagues.GetByIdAsync(LeagueId.Of(request.LeagueId), ct)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    //var hasOrganizations = await _orgs.ExistsAsync(o => o.LeagueId.Value == request.LeagueId, ct);
    //if (hasOrganizations)
    //  throw new ValidationException(
    //    "Cannot delete league that has organizations. Remove all organizations first.");

    // 3) Delete and persist
    _leagues.Remove(league);
    await _leagues.SaveChangesAsync(ct);

    // 4) Success envelope
    return ServiceResponse.Ok(true, "League successfully deleted.");
  }
}
