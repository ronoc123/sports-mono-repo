using Application.Common.Models;
using Application.Dto.Player;
using Contracts.Contracts;
using Contracts.Responses;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Players.Queries.GetAllPlayers;

public record GetAllPlayersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    LeagueId? LeagueId = null,
    OrganizationId? OrganizationId = null,
    string? Position = null,
    int? MinAge = null,
    int? MaxAge = null,
    bool? IsActive = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<ServiceResponse<PaginatedList<PlayerDto>>>;

