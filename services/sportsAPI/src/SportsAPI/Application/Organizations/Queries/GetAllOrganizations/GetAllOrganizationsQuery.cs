using Application.Common.Models;
using MediatR;

namespace Application.Organizations.Queries.GetAllOrganizations;

public record GetAllOrganizationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? LeagueId = null,
    string? Sport = null,
    bool? IsLocked = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<Result<PaginatedList<OrganizationDto>>>;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? TeamShortName { get; set; }
    public int? FormedYear { get; set; }
    public string? Sport { get; set; }
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Venue properties
    public string? Stadium { get; set; }
    public string? Location { get; set; }
    public int? StadiumCapacity { get; set; }
    
    // Media properties
    public string? BadgeUrl { get; set; }
    public string? LogoUrl { get; set; }
    
    // Social properties
    public string? Website { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    
    // Team colors
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }
    public string? Color3 { get; set; }
}
