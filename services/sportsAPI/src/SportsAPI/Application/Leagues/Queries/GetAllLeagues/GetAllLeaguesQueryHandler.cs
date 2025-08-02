using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Leagues.Queries.GetAllLeagues;

public class GetAllLeaguesQueryHandler : IRequestHandler<GetAllLeaguesQuery, Result<PaginatedList<LeagueDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllLeaguesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<LeagueDto>>> Handle(GetAllLeaguesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Leagues.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(l => l.Name.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
                _ => query.OrderBy(l => l.Name)
            };

            // Project to DTO with counts
            var dtoQuery = query.Select(l => new LeagueDto
            {
                Id = l.Id.Value,
                Name = l.Name,
                CreatedAt = l.CreatedAt ?? DateTime.MinValue,
                OrganizationCount = _context.Organizations.Count(o => o.LeagueId == l.Id),
                PlayerCount = _context.Players.Count(p => p.LeagueId == l.Id)
            });

            // Create paginated result
            var paginatedList = await PaginatedList<LeagueDto>.CreateAsync(
                dtoQuery, 
                request.PageNumber, 
                request.PageSize);

            return Result<PaginatedList<LeagueDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<LeagueDto>>.Failure($"Failed to retrieve leagues: {ex.Message}");
        }
    }
}
