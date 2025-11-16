using Application.Common.Models;
using Application.Dto.Player;
using MediatR;

namespace Application.Players.Queries.GetAllPlayers;

public record GetAllPlayersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? LeagueId = null,
    Guid? OrganizationId = null,
    string? Position = null,
    int? MinAge = null,
    int? MaxAge = null,
    bool? IsActive = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<Result<PaginatedList<PlayerDto>>>;

