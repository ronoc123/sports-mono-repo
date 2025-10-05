using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Queries.GetAllPlayers;

public class GetAllPlayersQueryHandler : IRequestHandler<GetAllPlayersQuery, Result<PaginatedList<PlayerDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPlayersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<PlayerDto>>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Players.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Position.ToLower().Contains(searchTerm));
            }

            if (request.LeagueId.HasValue)
            {
                var leagueId = LeagueId.Of(request.LeagueId.Value);
                query = query.Where(p => p.LeagueId == leagueId);
            }

            if (request.OrganizationId.HasValue)
            {
                var organizationId = OrganizationId.Of(request.OrganizationId.Value);
                query = query.Where(p => p.OrganizationId == organizationId);
            }

            if (!string.IsNullOrEmpty(request.Position))
            {
                query = query.Where(p => p.Position.ToLower() == request.Position.ToLower());
            }

            if (request.MinAge.HasValue)
            {
                query = query.Where(p => p.Age >= request.MinAge.Value);
            }

            if (request.MaxAge.HasValue)
            {
                query = query.Where(p => p.Age <= request.MaxAge.Value);
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    query = query.Where(p => p.Age >= 16 && p.Age <= 50);
                else
                    query = query.Where(p => p.Age < 16 || p.Age > 50);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "age" => request.SortDescending ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age),
                "position" => request.SortDescending ? query.OrderByDescending(p => p.Position) : query.OrderBy(p => p.Position),
                "updatedat" => request.SortDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            // Project to DTO
            var dtoQuery = query.Select(p => new PlayerDto
            {
                Id = p.Id.Value,
                Name = p.Name,
                Position = p.Position,
                ImageUrl = p.ImageUrl,
                Age = p.Age,
                UpdatedAt = p.UpdatedAt,
                LeagueId = p.LeagueId.Value,
                OrganizationId = p.OrganizationId != null ? p.OrganizationId.Value : null,
                IsActive = p.Age >= 16 && p.Age <= 50,
                IsVeteran = p.Age >= 35,
                IsYoungPlayer = p.Age <= 23,
                MarketValue = p.Age <= 20 ? 50000m : 
                             p.Age <= 25 ? 100000m : 
                             p.Age <= 30 ? 80000m : 
                             p.Age <= 35 ? 40000m : 20000m,
                // TODO: Add related data when navigation properties are properly configured
                // LeagueName = p.League.Name,
                // OrganizationName = p.Organization.Name,
                ActivePlayerOptionsCount = _context.PlayerOptions.Count(po => po.PlayerId == p.Id && po.ExpiresAt > DateTime.UtcNow)
            });

            // Create paginated result
            var paginatedList = await PaginatedList<PlayerDto>.CreateAsync(
                dtoQuery, 
                request.PageNumber, 
                request.PageSize);

            return Result<PaginatedList<PlayerDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<PlayerDto>>.Failure($"Failed to retrieve players: {ex.Message}");
        }
    }
}
