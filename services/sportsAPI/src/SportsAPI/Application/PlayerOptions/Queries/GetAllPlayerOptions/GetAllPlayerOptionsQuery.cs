using Application.Common.Models;
using MediatR;

namespace Application.PlayerOptions.Queries.GetAllPlayerOptions;

public record GetAllPlayerOptionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? OrganizationId = null,
    Guid? PlayerId = null,
    bool? IsActive = null,
    bool? IsExpired = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<Result<PaginatedList<PlayerOptionDto>>>;

public class PlayerOptionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Votes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid PlayerId { get; set; }
    public Guid OrganizationId { get; set; }
    
    // Business logic properties
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsPopular { get; set; }
    public bool IsTrending { get; set; }
    public int DaysRemaining { get; set; }
    public string PopularityLevel { get; set; } = string.Empty;
    public decimal EngagementScore { get; set; }
    
    // Related data
    public string? PlayerName { get; set; }
    public string? OrganizationName { get; set; }
}
