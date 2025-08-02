using Application.Common.Models;
using MediatR;

namespace Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "UserName",
    bool SortDescending = false
) : IRequest<Result<PaginatedList<UserDto>>>;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int VoteCount { get; set; }
    public int RedeemedCodeCount { get; set; }
}
