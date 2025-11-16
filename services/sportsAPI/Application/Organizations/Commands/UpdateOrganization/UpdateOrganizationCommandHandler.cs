using Contracts.Contracts;                 // ServiceResponse<T>
using BuildingBlocks.Exceptions;           // EntityNotFoundException (and your middleware maps it to 404)
using Domain.Repositories;                 // IOrganizationRepository
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects.ConcreteTypes;

namespace Application.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler
  : IRequestHandler<UpdateOrganizationCommand, ServiceResponse<bool>>
{
  private readonly IOrganizationRepository _orgs;

  public UpdateOrganizationCommandHandler(IOrganizationRepository orgs)
  {
    _orgs = orgs;
  }

  public async Task<ServiceResponse<bool>> Handle(UpdateOrganizationCommand request, CancellationToken ct)
  {
    var org = await _orgs.GetByIdAsync(OrganizationId.Of(request.OrganizationId), ct)
              ?? throw new ValidationException($"Organization '{request.OrganizationId}' not found.");

    //org.Name = request.Name?.Trim() ?? org.Name;

    //org.FormedYear = request.FormedYear;
    //org.Description = request.Description;

    org.UpdateTeamInfo(request.TeamId, request.TeamName, request.TeamShortName, request.Sport);

    _orgs.Update(org);
    await _orgs.SaveChangesAsync(ct);

    // 4) Success envelope
    return ServiceResponse.Ok(true, "Organization successfully updated.");
  }
}
