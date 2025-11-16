using System.Diagnostics;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Dto.League;
using Contracts.Contracts;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Leagues.Queries.GetAllLeagues;

public sealed class GetAllLeaguesQueryHandler
  : IRequestHandler<GetAllLeaguesQuery, ServiceResponse<PaginatedList<LeagueDto>>>
{
  private readonly IApplicationDbContext _context;
  private readonly ILeagueRepository _leagueRepository;

  public GetAllLeaguesQueryHandler(IApplicationDbContext context, ILeagueRepository leagueRepository)
  {
    _context = context;
    _leagueRepository = leagueRepository;
  }

  public async Task<ServiceResponse<PaginatedList<LeagueDto>>> Handle(
      GetAllLeaguesQuery request,
      CancellationToken cancellationToken)
  {
       var leagues = _leagueRepository.Query();

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
            OrganizationCount = _context.Organizations.Count(o => o.LeagueId == l.Id),
            PlayerCount = _context.Players.Count(p => p.LeagueId == l.Id)
          });

      var paginatedList = await PaginatedList<LeagueDto>.CreateAsync(
          dtoQuery,
          request.PageNumber,
          request.PageSize);
      return ServiceResponse.Ok(paginatedList, string.Empty);
  }
}



