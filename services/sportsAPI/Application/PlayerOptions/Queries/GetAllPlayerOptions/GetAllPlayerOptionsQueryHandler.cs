
using Application.Common.Models;
using Contracts.Contracts;
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
    //private readonly IPlayerOptionRepository _readRepo;

    public GetAllPlayerOptionsQueryHandler(
      //IPlayerOptionRepository readRepo
      )
    {
      //_readRepo = readRepo;
    }

    public async Task<ServiceResponse<PaginatedList<PlayerOptionDto>>> Handle(
        GetAllPlayerOptionsQuery request,
        CancellationToken cancellationToken)
    {
      //var now = DateTime.UtcNow;

      //var query = _readRepo.Query(asNoTracking: true);

      //// Filters
      //if (!string.IsNullOrWhiteSpace(request.SearchTerm))
      //{
      //  var search = request.SearchTerm.Trim().ToLower();
      //  query = query.Where(po =>
      //      po.Title.ToLower().Contains(search) ||
      //      po.Description.ToLower().Contains(search));
      //}

      //if (request.OrganizationId.HasValue)
      //{
      //  var organizationId = OrganizationId.Of(request.OrganizationId.Value);
      //  query = query.Where(po => po.OrganizationId == organizationId);
      //}

      //if (request.PlayerId.HasValue)
      //{
      //  var playerId = PlayerId.Of(request.PlayerId.Value);
      //  query = query.Where(po => po.PlayerId == playerId);
      //}

      //if (request.IsActive.HasValue)
      //{
      //  query = request.IsActive.Value
      //      ? query.Where(po => po.ExpiresAt > now)
      //      : query.Where(po => po.ExpiresAt <= now);
      //}

      //if (request.IsExpired.HasValue)
      //{
      //  query = request.IsExpired.Value
      //      ? query.Where(po => po.ExpiresAt <= now)
      //      : query.Where(po => po.ExpiresAt > now);
      //}

      //// Sorting
      //query = request.SortBy?.ToLower() switch
      //{
      //  "title" => request.SortDescending ? query.OrderByDescending(po => po.Title) : query.OrderBy(po => po.Title),
      //  "votes" => request.SortDescending ? query.OrderByDescending(po => po.Votes) : query.OrderBy(po => po.Votes),
      //  "expiresat" => request.SortDescending ? query.OrderByDescending(po => po.ExpiresAt) : query.OrderBy(po => po.ExpiresAt),
      //  "createdat" => request.SortDescending ? query.OrderByDescending(po => po.CreatedAt) : query.OrderBy(po => po.CreatedAt),
      //  _ => query.OrderByDescending(po => po.CreatedAt)
      //};

      //// Projection
      //var dtoQuery = query.Select(po => new PlayerOptionDto
      //{
      //  Id = po.Id.Value,
      //  Title = po.Title,
      //  Description = po.Description,
      //  Votes = po.Votes,
      //  ExpiresAt = po.ExpiresAt,
      //  CreatedAt = po.CreatedAt ?? DateTime.MinValue,
      //  PlayerId = po.PlayerId.Value,
      //  OrganizationId = po.OrganizationId.Value,
      //  IsActive = po.ExpiresAt > now,
      //  IsExpired = po.ExpiresAt <= now,
      //  IsPopular = po.Votes >= 100,
      //  IsTrending = po.Votes >= 50 && po.ExpiresAt > now,
      //  DaysRemaining = po.ExpiresAt > now
      //        ? (int)Math.Ceiling((po.ExpiresAt - now).TotalDays)
      //        : 0,
      //  PopularityLevel = po.Votes >= 1000 ? "Viral" :
      //                      po.Votes >= 500 ? "Very Popular" :
      //                      po.Votes >= 100 ? "Popular" :
      //                      po.Votes >= 50 ? "Trending" :
      //                      po.Votes >= 10 ? "Active" : "New",
      //  EngagementScore = po.CreatedAt.HasValue && po.CreatedAt.Value < now
      //        ? (decimal)(po.Votes / Math.Max(1, (now - po.CreatedAt.Value).TotalDays))
      //        : 0,
      //  // PlayerName / OrganizationName can be added once nav props are configured
      //});

      //// Pagination
      //var paginated = await PaginatedList<PlayerOptionDto>.CreateAsync(
      //    dtoQuery,
      //    request.PageNumber,
      //    request.PageSize);

      //return ServiceResponse.Ok(paginated, "Player options retrieved.");
      return null;
      }
  }
}
