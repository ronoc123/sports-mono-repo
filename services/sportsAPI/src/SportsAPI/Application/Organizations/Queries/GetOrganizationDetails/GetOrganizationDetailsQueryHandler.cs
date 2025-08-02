using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Queries.GetOrganizationDetails;

public class GetOrganizationDetailsQueryHandler : IRequestHandler<GetOrganizationDetailsQuery, Result<OrganizationDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationDetailsQueryHandler(IApplicationDbContext context, IOrganizationRepository organizationRepository)
    {
        _context = context;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationDetailsDto>> Handle(GetOrganizationDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId);
            if (organization == null)
            {
                return Result<OrganizationDetailsDto>.Failure("Organization not found");
            }

            // Get player options for this organization
            var playerOptions = await _context.PlayerOptions
                .Where(po => po.OrganizationId.Value == request.OrganizationId)
                .Include(po => po.PlayerId)
                .ToListAsync(cancellationToken);

            var dto = new OrganizationDetailsDto
            {
                Id = organization.Id.Value,
                Name = organization.Name,
                TeamId = organization.TeamId,
                TeamName = organization.TeamName,
                TeamShortName = organization.TeamShortName,
                FormedYear = organization.FormedYear,
                Sport = organization.Sport,
                Stadium = organization.Venue.Stadium,
                Location = organization.Venue.Location,
                StadiumCapacity = organization.Venue.Capacity,
                Website = organization.SocialLinks.Website,
                Facebook = organization.SocialLinks.Facebook,
                Twitter = organization.SocialLinks.Twitter,
                Instagram = organization.SocialLinks.Instagram,
                Description = organization.Description,
                Color1 = organization.TeamColors.Primary,
                Color2 = organization.TeamColors.Secondary,
                Color3 = organization.TeamColors.Tertiary,
                BadgeUrl = organization.MediaAssets.BadgeUrl,
                LogoUrl = organization.MediaAssets.LogoUrl,
                Fanart1Url = organization.MediaAssets.Fanart1Url,
                Fanart2Url = organization.MediaAssets.Fanart2Url,
                Fanart3Url = organization.MediaAssets.Fanart3Url,
                PlayerOptions = playerOptions.Select(po => new PlayerOptionDto
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
                        // Note: You'll need to load player details separately or include them in the query
                        Name = "Player Name", // TODO: Load from Player entity
                        Position = "Position", // TODO: Load from Player entity
                        ImageUrl = "ImageUrl", // TODO: Load from Player entity
                        UpdatedAt = DateTime.Now, // TODO: Load from Player entity
                        Age = 0 // TODO: Load from Player entity
                    }
                }).ToList()
            };

            return Result<OrganizationDetailsDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrganizationDetailsDto>.Failure($"Failed to get organization details: {ex.Message}");
        }
    }
}
