using Application.Common.Models;
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

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid LeagueId { get; set; }
    public Guid? OrganizationId { get; set; }
    
    // Business logic properties
    public bool IsActive { get; set; }
    public bool IsVeteran { get; set; }
    public bool IsYoungPlayer { get; set; }
    public decimal MarketValue { get; set; }
    
    // Related data
    public string? LeagueName { get; set; }
    public string? OrganizationName { get; set; }
    public int ActivePlayerOptionsCount { get; set; }
}
