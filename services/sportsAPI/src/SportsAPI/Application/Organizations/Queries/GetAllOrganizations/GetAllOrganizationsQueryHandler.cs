using Contracts.Contracts;                 // ServiceResponse<T>
using Domain.Repositories;                 // IOrganizationRepository
using Domain.ValueObjects.ConcreteTypes;   // LeagueId
using Application.Common.Models;           // PaginatedList<T>
using MediatR;
using Microsoft.EntityFrameworkCore;       // for EF Core query ops on IQueryable

namespace Application.Organizations.Queries.GetAllOrganizations;

public sealed class GetAllOrganizationsQueryHandler
  : IRequestHandler<GetAllOrganizationsQuery, ServiceResponse<PaginatedList<OrganizationDto>>>
{
  private readonly IOrganizationRepository _orgs;

  public GetAllOrganizationsQueryHandler(IOrganizationRepository orgs)
  {
    _orgs = orgs;
  }

  public async Task<ServiceResponse<PaginatedList<OrganizationDto>>> Handle(
      GetAllOrganizationsQuery request,
      CancellationToken cancellationToken)
  {

    var query = _orgs.Query();

    if (!string.IsNullOrWhiteSpace(request.SearchTerm))
    {
      var term = request.SearchTerm.ToLower();
      query = query.Where(o =>
          o.Name.ToLower().Contains(term) ||
          (o.TeamName != null && o.TeamName.ToLower().Contains(term)) ||
          (o.Sport != null && o.Sport.ToLower().Contains(term)) ||
          (o.Description != null && o.Description.ToLower().Contains(term)));
    }

    if (request.LeagueId.HasValue)
    {
      var leagueId = LeagueId.Of(request.LeagueId.Value);
      query = query.Where(o => o.LeagueId == leagueId);
    }

    if (!string.IsNullOrWhiteSpace(request.Sport))
    {
      var sport = request.Sport.ToLower();
      query = query.Where(o => o.Sport != null && o.Sport.ToLower() == sport);
    }

    // Sort
    query = request.SortBy?.ToLower() switch
    {
      "name" => request.SortDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name),
      "createdat" => request.SortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
      "formedyear" => request.SortDescending ? query.OrderByDescending(o => o.FormedYear) : query.OrderBy(o => o.FormedYear),
      "sport" => request.SortDescending ? query.OrderByDescending(o => o.Sport) : query.OrderBy(o => o.Sport),
      "teamname" => request.SortDescending ? query.OrderByDescending(o => o.TeamName) : query.OrderBy(o => o.TeamName),
      _ => query.OrderBy(o => o.Name)
    };

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
      CreatedAt = o.CreatedAt ?? DateTime.MinValue,
    });

    var page = await PaginatedList<OrganizationDto>.CreateAsync(
      dtoQuery, request.PageNumber, request.PageSize);

    return ServiceResponse.Ok(page);
  }
}
