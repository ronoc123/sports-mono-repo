using Contracts.Contracts;             
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Organizations.Commands.DeleteOrganization;

public sealed class DeleteOrganizationCommandHandler
  : IRequestHandler<DeleteOrganizationCommand, ServiceResponse<bool>>
{
  private readonly IOrganizationRepository _orgs;

  public DeleteOrganizationCommandHandler(IOrganizationRepository orgs)
  {
    _orgs = orgs;
  }

  public async Task<ServiceResponse<bool>> Handle(DeleteOrganizationCommand request, CancellationToken ct)
  {
    var org = await _orgs.GetByIdAsync(OrganizationId.Of(request.OrganizationId), ct)
              ?? throw new ValidationException($"Organization '{request.OrganizationId}' not found.");


    _orgs.Remove(org);
    await _orgs.SaveChangesAsync(ct);

    return ServiceResponse.Ok(true, "Organization successfully deleted.");
  }
}
