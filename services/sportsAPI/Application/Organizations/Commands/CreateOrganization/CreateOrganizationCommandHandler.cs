using BuildingBlocks.Exceptions;           // EntityNotFoundException
using Contracts.Contracts;                 // ServiceResponse<T>
using Domain.Organizations;                // Organization
using Domain.Organizations.Entities;       // Venue, MediaAssets, SocialLinks, TeamColors
using Domain.Repositories;                 // ILeagueRepository, IOrganizationRepository
using Domain.ValueObjects;
using Domain.ValueObjects.ConcreteTypes;   // OrganizationId
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler
  : IRequestHandler<CreateOrganizationCommand, ServiceResponse<Guid>>
{
  private readonly ILeagueRepository _leagues;
  private readonly IOrganizationRepository _orgs;

  public CreateOrganizationCommandHandler(
      ILeagueRepository leagues,
      IOrganizationRepository orgs)
  {
    _leagues = leagues;
    _orgs = orgs;
  }

  public async Task<ServiceResponse<Guid>> Handle(CreateOrganizationCommand request, CancellationToken ct)
  {
    var league = await _leagues.GetByIdAsync(LeagueId.Of(request.LeagueId), ct)
                 ?? throw new ValidationException($"League '{request.LeagueId}' not found.");

    // 2) Build value objects (use empty strings/defaults as per your current model)
    var venue = new Venue(
      request.Stadium ?? string.Empty,
      request.Location ?? string.Empty,
      request.StadiumCapacity ?? 0
    );

    var mediaAssets = new MediaAssets(
      request.BadgeUrl ?? string.Empty,
      request.LogoUrl ?? string.Empty,
      request.Fanart1Url ?? string.Empty,
      request.Fanart2Url ?? string.Empty,
      request.Fanart3Url ?? string.Empty
    );

    var socialLinks = new SocialLinks(
      request.Website ?? string.Empty,
      request.Facebook ?? string.Empty,
      request.Twitter ?? string.Empty,
      request.Instagram ?? string.Empty
    );

    var teamColors = new TeamColors(
      request.Color1 ?? string.Empty,
      request.Color2 ?? string.Empty,
      request.Color3 ?? string.Empty
    );

    // 3) Create aggregate
    var orgId = OrganizationId.Of(Guid.NewGuid());
    var organization = Organization.Create(
      orgId,
      LeagueId.Of(request.LeagueId),
      request.Name,
      request.TeamId,
      request.TeamName,
      request.TeamShortName,
      request.FormedYear,
      request.Sport,
      venue,
      mediaAssets,
      socialLinks,
      teamColors,
      request.Description
    );

    // Optionally: attach to league via a domain method if/when you model it
    // league.AddOrganization(organization);

    // 4) Persist
    await _orgs.AddAsync(organization, ct);
    await _orgs.SaveChangesAsync(ct);

    // 5) Success envelope (return the new ID)
    return ServiceResponse.Ok(orgId.Value, "Organization successfully created.");
  }
}
