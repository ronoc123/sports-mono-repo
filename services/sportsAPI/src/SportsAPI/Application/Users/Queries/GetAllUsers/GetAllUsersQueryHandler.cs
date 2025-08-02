using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.UserName.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "username" => request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderBy(u => u.UserName)
            };

            // Project to DTO with counts
            var dtoQuery = query.Select(u => new UserDto
            {
                Id = u.Id.Value,
                Email = u.Email,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                VoteCount = _context.Votes.Count(v => v.CreatedBy == u.Id.Value.ToString()),
                RedeemedCodeCount = _context.Codes.Count(c => c.RedeemerId != null && c.RedeemerId == u.Id)
            });

            // Create paginated result
            var paginatedList = await PaginatedList<UserDto>.CreateAsync(
                dtoQuery, 
                request.PageNumber, 
                request.PageSize);

            return Result<PaginatedList<UserDto>>.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<UserDto>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}
