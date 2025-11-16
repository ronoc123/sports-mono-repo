using Contracts.Contracts;                 // ServiceResponse<T>
using BuildingBlocks.Exceptions;           // EntityNotFoundException (and your middleware maps it to 404)
using Domain.Repositories;                 // IOrganizationRepository
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects.ConcreteTypes;
using Domain.Organizations;

namespace Application.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler
  : IRequestHandler<UpdateOrganizationCommand, ServiceResponse<bool>>
{
  private readonly IRepository _repo;

  public UpdateOrganizationCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

  public async Task<ServiceResponse<bool>> Handle(UpdateOrganizationCommand request, CancellationToken ct)
  {
    var org = await _repo.GetByIdAsync<Organization,OrganizationId>(request.OrganizationId, ct)
              ?? throw new ValidationException($"Organization '{request.OrganizationId}' not found.");

    org.UpdateTeamInfo(request.TeamId, request.TeamName, request.TeamShortName, request.Sport);

    _repo.Update(org);
    await _repo.SaveChangesAsync(ct);

    return ServiceResponse.Ok(true, "Organization successfully updated.");
  }
}
