using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var dto = new UserDto
            {
                Id = user.Id.Value,
                Email = user.Email,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                VotesAvailable = user.VotesAvailable.Select(v => new UserVoteDto
                {
                    OrganizationId = v.OrganizationId.Value,
                    VotesRemaining = v.VotesRemaining
                }).ToList(),
                Votes = user.Votes.Select(v => new VoteDto
                {
                    Id = v.Id.Value,
                    CreatedAt = v.CreatedAt ?? DateTime.MinValue
                }).ToList(),
                RedeemedCodes = user.Codes.Select(c => new CodeDto
                {
                    Id = c.Id.Value,
                    VotesAwarded = c.VotesAwarded
                }).ToList()
            };

            return Result<UserDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}");
        }
    }
}
