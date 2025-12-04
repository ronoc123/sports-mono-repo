using Application.Common.Models;           // PaginatedList<T>
using Application.Dto.Organization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.Contracts;                 // ServiceResponse<T>
using Contracts.Responses;
using Domain.Organizations;       // for EF Core query ops on IQueryable
using Domain.Repositories;                 // IOrganizationRepository
using Domain.ValueObjects.ConcreteTypes;   // LeagueId
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Queries.GetAllOrganizations;

public sealed class GetAllOrganizationsQueryHandler
  : IRequestHandler<GetAllOrganizationsQuery, ServiceResponse<PaginatedList<OrganizationDto>>>
{

  private readonly IRepository _repo;

  private readonly IMapper _mapper;
  public GetAllOrganizationsQueryHandler(IRepository repo, IMapper mapper)
  {
    _repo = repo;
    _mapper = mapper;
  }

  public async Task<ServiceResponse<PaginatedList<OrganizationDto>>> Handle(
      GetAllOrganizationsQuery request,
      CancellationToken cancellationToken)
  {

    var query = _repo.Query<Organization>();

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

    var dtoQuery = query.ProjectTo<OrganizationDto>(_mapper.ConfigurationProvider);

    var page = await PaginatedListFactory.CreateAsync(
      dtoQuery, request.PageNumber, request.PageSize);

    return ServiceResponse.Ok(page);
  }
}
