using Contracts.Contracts;
using Domain.Organizations;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Organizations.Commands.DeleteOrganization;

public sealed class DeleteOrganizationCommandHandler
  : IRequestHandler<DeleteOrganizationCommand, ServiceResponse<bool>>
{
  private readonly IRepository _repo;

  public DeleteOrganizationCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

  public async Task<ServiceResponse<bool>> Handle(DeleteOrganizationCommand request, CancellationToken ct)
  {
    var org = await _repo.GetByIdAsync<Organization, OrganizationId>(request.OrganizationId, ct)
              ?? throw new ValidationException($"Organization '{request.OrganizationId}' not found.");
    _repo.Remove(org);
    await _repo.SaveChangesAsync(ct);

    return ServiceResponse.Ok(true, "Organization successfully deleted.");
  }
}
