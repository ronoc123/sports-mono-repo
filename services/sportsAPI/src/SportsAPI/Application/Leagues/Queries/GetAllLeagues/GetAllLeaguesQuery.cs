using Application.Common.Models;
using MediatR;

namespace Application.Leagues.Queries.GetAllLeagues;

public record GetAllLeaguesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<Result<PaginatedList<LeagueDto>>>;

public class LeagueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OrganizationCount { get; set; }
    public int PlayerCount { get; set; }
}
