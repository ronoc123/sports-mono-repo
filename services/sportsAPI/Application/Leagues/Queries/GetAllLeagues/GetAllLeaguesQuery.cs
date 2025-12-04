using Application.Common.Models;
using Application.Dto.League;
using Contracts.Contracts;
using Contracts.Responses;
using MediatR;

namespace Application.Leagues.Queries.GetAllLeagues;

public record GetAllLeaguesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<ServiceResponse<PaginatedList<LeagueDto>>>;
