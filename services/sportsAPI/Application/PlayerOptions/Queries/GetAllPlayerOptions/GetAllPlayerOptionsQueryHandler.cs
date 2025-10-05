using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.PlayerOptions.Queries.GetAllPlayerOptions;

public class GetAllPlayerOptionsQueryHandler : IRequestHandler<GetAllPlayerOptionsQuery, Result<PaginatedList<PlayerOptionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPlayerOptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<PlayerOptionDto>>> Handle(GetAllPlayerOptionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.PlayerOptions.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(po => 
                    po.Title.ToLower().Contains(searchTerm) ||
                    po.Description.ToLower().Contains(searchTerm));
            }

            if (request.OrganizationId.HasValue)
            {
                var organizationId = OrganizationId.Of(request.OrganizationId.Value);
                query = query.Where(po => po.OrganizationId == organizationId);
            }

            if (request.PlayerId.HasValue)
            {
                var playerId = PlayerId.Of(request.PlayerId.Value);
                query = query.Where(po => po.PlayerId == playerId);
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    query = query.Where(po => po.ExpiresAt > DateTime.UtcNow);
                else
                    query = query.Where(po => po.ExpiresAt <= DateTime.UtcNow);
            }

            if (request.IsExpired.HasValue)
            {
                if (request.IsExpired.Value)
                    query = query.Where(po => po.ExpiresAt <= DateTime.UtcNow);
                else
                    query = query.Where(po => po.ExpiresAt > DateTime.UtcNow);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(po => po.Title) : query.OrderBy(po => po.Title),
                "votes" => request.SortDescending ? query.OrderByDescending(po => po.Votes) : query.OrderBy(po => po.Votes),
                "expiresat" => request.SortDescending ? query.OrderByDescending(po => po.ExpiresAt) : query.OrderBy(po => po.ExpiresAt),
                "createdat" => request.SortDescending ? query.OrderByDescending(po => po.CreatedAt) : query.OrderBy(po => po.CreatedAt),
                _ => query.OrderByDescending(po => po.CreatedAt)
            };

            // Project to DTO
            var dtoQuery = query.Select(po => new PlayerOptionDto
            {
                Id = po.Id.Value,
                Title = po.Title,
                Description = po.Description,
                Votes = po.Votes,
                ExpiresAt = po.ExpiresAt,
                CreatedAt = po.CreatedAt ?? DateTime.MinValue,
                PlayerId = po.PlayerId.Value,
                OrganizationId = po.OrganizationId.Value,
                IsActive = po.ExpiresAt > DateTime.UtcNow,
                IsExpired = po.ExpiresAt <= DateTime.UtcNow,
                IsPopular = po.Votes >= 100,
                IsTrending = po.Votes >= 50 && po.ExpiresAt > DateTime.UtcNow,
                DaysRemaining = po.ExpiresAt > DateTime.UtcNow ? 
                    (int)Math.Ceiling((po.ExpiresAt - DateTime.UtcNow).TotalDays) : 0,
                PopularityLevel = po.Votes >= 1000 ? "Viral" :
                                po.Votes >= 500 ? "Very Popular" :
                                po.Votes >= 100 ? "Popular" :
                                po.Votes >= 50 ? "Trending" :
                                po.Votes >= 10 ? "Active" : "New",
                EngagementScore = po.CreatedAt.HasValue && po.CreatedAt.Value < DateTime.UtcNow ?
                    (decimal)(po.Votes / Math.Max(1, (DateTime.UtcNow - po.CreatedAt.Value).TotalDays)) : 0,
                // TODO: Add related data when navigation properties are properly configured
                // PlayerName = po.Player.Name,
                // OrganizationName = po.Organization.Name
            });

            // Create paginated result
            var paginatedList = await PaginatedList<PlayerOptionDto>.CreateAsync(
                dtoQuery, 
                request.PageNumber, 
                request.PageSize);

            return Result<PaginatedList<PlayerOptionDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<PlayerOptionDto>>.Failure($"Failed to retrieve player options: {ex.Message}");
        }
    }
}
