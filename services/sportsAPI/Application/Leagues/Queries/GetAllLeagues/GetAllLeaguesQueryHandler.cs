using Application.Dto.League;
using Contracts.Contracts;
using Domain.Leagues;
using Domain.Repositories;
using MediatR;

namespace Application.Leagues.Queries.GetAllLeagues;

public sealed class GetAllLeaguesQueryHandler : IRequestHandler<GetAllLeaguesQuery, ServiceResponse<List<LeagueDto>>>
{
    private readonly IRepository _repo;

    public GetAllLeaguesQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<LeagueDto>>> Handle(GetAllLeaguesQuery request, CancellationToken cancellationToken)
    {
        var leagues = _repo.Query<League>();

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            leagues = leagues.Where(l => l.Name.ToLower().Contains(term));
        }

        // Sort
        leagues = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? leagues.OrderByDescending(l => l.Name) : leagues.OrderBy(l => l.Name),
            "createdat" => request.SortDescending ? leagues.OrderByDescending(l => l.CreatedAt) : leagues.OrderBy(l => l.CreatedAt),
            _ => leagues.OrderBy(l => l.Name)
        };

        var dtoQuery =
            leagues.Select(l => new LeagueDto
            {
                Id = l.Id.Value,
                Name = l.Name,
                CreatedAt = l.CreatedAt ?? DateTime.MinValue,
            });
        var paginatedList = await PaginatedListFactory.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize);

        return ServiceResponse.Ok(dtoQuery.ToList(), string.Empty);

    }
}



