using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Dto.Player;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.Player;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Queries.GetAllPlayers;

public class GetAllPlayersQueryHandler : IRequestHandler<GetAllPlayersQuery, ServiceResponse<PaginatedList<PlayerDto>>>
{
    private readonly IRepository _repo;

    public GetAllPlayersQueryHandler(IRepository repo)
    {
    _repo = repo;
    }

    public async Task<ServiceResponse<PaginatedList<PlayerDto>>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Query<Player>().Where(p => p.LeagueId == request.LeagueId);
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
          var searchTerm = request.SearchTerm.ToLower();
          query = query.Where(p =>
              p.Name.ToLower().Contains(searchTerm) ||
              p.Position.ToLower().Contains(searchTerm));
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

        var dtoQuery = query.Select(p => new PlayerDto
        {
          Id = p.Id.Value,
          Name = p.Name,
          Position = p.Position,
          ImageUrl = p.ImageUrl,
          Age = p.Age,
          LeagueId = p.LeagueId.Value,
          OrganizationId = p.OrganizationId != null ? p.OrganizationId.Value : null,
          IsActive = p.Age >= 16 && p.Age <= 50,
          IsVeteran = p.Age >= 35,
          IsYoungPlayer = p.Age <= 23,
        });

        // Create paginated result
        var paginatedList = await PaginatedListFactory.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize);

        return ServiceResponse.Ok(paginatedList, "Success");
  }
}
