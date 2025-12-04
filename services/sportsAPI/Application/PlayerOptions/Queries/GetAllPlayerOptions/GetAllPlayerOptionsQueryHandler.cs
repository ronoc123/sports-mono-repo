
using Application.Common.Models;
using Application.Dto.Organization;
using Application.Dto.PlayerOption;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.PlayerOption;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.PlayerOptions.Queries.GetAllPlayerOptions
{
  public class GetAllPlayerOptionsQueryHandler
      : IRequestHandler<GetAllPlayerOptionsQuery, ServiceResponse<PaginatedList<PlayerOptionDto>>>
  {
    private readonly IRepository _repo;
    private readonly IMapper _mapper;

    public GetAllPlayerOptionsQueryHandler(IRepository repo, IMapper mapper)
    {
      _repo = repo;
      _mapper = mapper;
    }


    public async Task<ServiceResponse<PaginatedList<PlayerOptionDto>>> Handle(GetAllPlayerOptionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query = _repo.Query<PlayerOption>();

        // Filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
          var search = request.SearchTerm.Trim().ToLower();
          query = query.Where(po =>
              po.Title.ToLower().Contains(search) ||
              po.Description.ToLower().Contains(search));
        }

      if (request.OrganizationId.HasValue)
      {
        query = query.Where(po => po.OrganizationId == OrganizationId.Of(request.OrganizationId.Value));
      }

      if (request.PlayerId.HasValue)
      {
        query = query.Where(po => po.PlayerId == PlayerId.Of(request.PlayerId.Value));
      }

      if (request.IsActive.HasValue)
        {
          query = request.IsActive.Value
              ? query.Where(po => po.ExpiresAt > now)
              : query.Where(po => po.ExpiresAt <= now);
        }

        if (request.IsExpired.HasValue)
        {
          query = request.IsExpired.Value
              ? query.Where(po => po.ExpiresAt <= now)
              : query.Where(po => po.ExpiresAt > now);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
          "title" => request.SortDescending ? query.OrderByDescending(po => po.Title) : query.OrderBy(po => po.Title),
          "votes" => request.SortDescending ? query.OrderByDescending(po => po.Votes) : query.OrderBy(po => po.Votes),
          "expiresat" => request.SortDescending ? query.OrderByDescending(po => po.ExpiresAt) : query.OrderBy(po => po.ExpiresAt),
          "createdat" => request.SortDescending ? query.OrderByDescending(po => po.CreatedAt) : query.OrderBy(po => po.CreatedAt),
          _ => query.OrderByDescending(po => po.CreatedAt)
        };

      var dtoQuery = query.ProjectTo<PlayerOptionDto>(_mapper.ConfigurationProvider);
      // Projection


        // Pagination
        var paginated = await PaginatedListFactory.CreateAsync(
            dtoQuery,
            request.PageNumber,
            request.PageSize);

        return ServiceResponse.Ok(paginated, "Player options retrieved.");

      }
  }
}
