using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;           // EntityNotFoundException
using Contracts.Contracts;                 // ServiceResponse<T>
using Domain.Repositories;                 // IOrganizationRepository
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.Organizations.Queries.GetOrganizationDetails;

public sealed class GetOrganizationDetailsQueryHandler
  : IRequestHandler<GetOrganizationDetailsQuery, ServiceResponse<OrganizationDetailsDto>>
{
  private readonly IOrganizationRepository _orgs;
  private readonly IApplicationDbContext _context;

  public GetOrganizationDetailsQueryHandler(
      IOrganizationRepository orgs,
      IApplicationDbContext context)
  {
    _orgs = orgs;
    _context = context;
  }

  public async Task<ServiceResponse<OrganizationDetailsDto>> Handle(
      GetOrganizationDetailsQuery request,
      CancellationToken ct)
  {
    var org = await _orgs.GetByIdAsync(request.OrganizationId, ct)
              ?? throw new ValidationException($"Organization '{request.OrganizationId.Value}' not found.");

    var options = org.PlayerOptions;


    var dto = new OrganizationDetailsDto
    {
      Id = org.Id.Value,
      Name = org.Name,
      TeamId = org.TeamId,
      TeamName = org.TeamName,
      TeamShortName = org.TeamShortName,
      FormedYear = org.FormedYear,
      Sport = org.Sport,
      Stadium = org.Venue.Stadium,
      Location = org.Venue.Location,
      StadiumCapacity = org.Venue.Capacity,
      Website = org.SocialLinks.Website,
      Facebook = org.SocialLinks.Facebook,
      Twitter = org.SocialLinks.Twitter,
      Instagram = org.SocialLinks.Instagram,
      Description = org.Description,
      Color1 = org.TeamColors.Primary,
      Color2 = org.TeamColors.Secondary,
      Color3 = org.TeamColors.Tertiary,
      BadgeUrl = org.MediaAssets.BadgeUrl,
      LogoUrl = org.MediaAssets.LogoUrl,
      Fanart1Url = org.MediaAssets.Fanart1Url,
      Fanart2Url = org.MediaAssets.Fanart2Url,
      Fanart3Url = org.MediaAssets.Fanart3Url,
      PlayerOptions = options.Select(po => new PlayerOptionDto
      {
        Id = po.Id.Value,
        Title = po.Title,
        Description = po.Description,
        Votes = po.Votes,
        CreatedAt = po.CreatedAt ?? DateTime.MinValue,
        ExpiresAt = po.ExpiresAt,
        Player = new PlayerDto
        {
          Id = po.PlayerId.Value,
          // TODO: replace placeholders by loading Player via repo or projection join
          Name = "Player Name",
          Position = "Position",
          ImageUrl = "ImageUrl",
          UpdatedAt = DateTime.UtcNow,
          Age = 0
        }
      }).ToList()
    };

    // 4) Success envelope
    return ServiceResponse.Ok(dto);
  }
}
