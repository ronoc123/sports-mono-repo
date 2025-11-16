using Contracts.Contracts;        
using Domain.Leagues;
using Domain.Organizations;           
using Domain.Repositories;                 
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler
  : IRequestHandler<CreateOrganizationCommand, ServiceResponse<OrganizationId>>
{
  private readonly IRepository _repo;

  public CreateOrganizationCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

  public async Task<ServiceResponse<OrganizationId>> Handle(CreateOrganizationCommand request, CancellationToken ct)
  {
    var league = await _repo.GetByIdAsync<League>(ct, request.LeagueId)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    var organization = Organization.Create(
        league.Id,
        request.Name,
        request.TeamId,
        request.TeamName,
        request.TeamShortName,
        request.FormedYear,
        request.Sport,
        request.Stadium,
        request.Location,
        request.StadiumCapacity,
        request.BadgeUrl,
        request.LogoUrl,
        request.Fanart1Url,
        request.Fanart2Url,
        request.Fanart3Url,
        request.Website,
        request.Facebook,
        request.Twitter,
        request.Instagram,
        request.Color1,
        request.Color2,
        request.Color3,
        request.Description
    );

    await _repo.AddAsync(organization, ct);
    await _repo.SaveChangesAsync(ct);

    return ServiceResponse.Ok(organization.Id, "Organization successfully created.");
  }
}
