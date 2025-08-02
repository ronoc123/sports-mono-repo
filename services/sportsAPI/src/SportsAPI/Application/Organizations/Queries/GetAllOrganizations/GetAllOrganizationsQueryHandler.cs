using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Queries.GetAllOrganizations;

public class GetAllOrganizationsQueryHandler : IRequestHandler<GetAllOrganizationsQuery, Result<PaginatedList<OrganizationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllOrganizationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<OrganizationDto>>> Handle(GetAllOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Organizations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(o => 
                    o.Name.ToLower().Contains(searchTerm) ||
                    (o.TeamName != null && o.TeamName.ToLower().Contains(searchTerm)) ||
                    (o.Sport != null && o.Sport.ToLower().Contains(searchTerm)) ||
                    (o.Description != null && o.Description.ToLower().Contains(searchTerm)));
            }

            if (request.LeagueId.HasValue)
            {
                var leagueId = LeagueId.Of(request.LeagueId.Value);
                query = query.Where(o => o.LeagueId == leagueId);
            }

            if (!string.IsNullOrEmpty(request.Sport))
            {
                query = query.Where(o => o.Sport != null && o.Sport.ToLower() == request.Sport.ToLower());
            }

            if (request.IsLocked.HasValue)
            {
                query = query.Where(o => o.IsLocked == request.IsLocked.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
                "formedyear" => request.SortDescending ? query.OrderByDescending(o => o.FormedYear) : query.OrderBy(o => o.FormedYear),
                "sport" => request.SortDescending ? query.OrderByDescending(o => o.Sport) : query.OrderBy(o => o.Sport),
                "teamname" => request.SortDescending ? query.OrderByDescending(o => o.TeamName) : query.OrderBy(o => o.TeamName),
                _ => query.OrderBy(o => o.Name)
            };

            // Project to DTO
            var dtoQuery = query.Select(o => new OrganizationDto
            {
                Id = o.Id.Value,
                LeagueId = o.LeagueId.Value,
                Name = o.Name,
                TeamId = o.TeamId,
                TeamName = o.TeamName,
                TeamShortName = o.TeamShortName,
                FormedYear = o.FormedYear,
                Sport = o.Sport,
                Description = o.Description,
                IsLocked = o.IsLocked,
                CreatedAt = o.CreatedAt ?? DateTime.MinValue,
                // TODO: Add value object properties when they're properly configured
                // Stadium = o.Venue.Stadium,
                // Location = o.Venue.Location,
                // StadiumCapacity = o.Venue.Capacity,
                // BadgeUrl = o.MediaAssets.BadgeUrl,
                // LogoUrl = o.MediaAssets.LogoUrl,
                // Website = o.SocialLinks.Website,
                // Facebook = o.SocialLinks.Facebook,
                // Twitter = o.SocialLinks.Twitter,
                // Instagram = o.SocialLinks.Instagram,
                // Color1 = o.TeamColors.Primary,
                // Color2 = o.TeamColors.Secondary,
                // Color3 = o.TeamColors.Tertiary
            });

            // Create paginated result
            var paginatedList = await PaginatedList<OrganizationDto>.CreateAsync(
                dtoQuery, 
                request.PageNumber, 
                request.PageSize);

            return Result<PaginatedList<OrganizationDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<OrganizationDto>>.Failure($"Failed to retrieve organizations: {ex.Message}");
        }
    }
}
