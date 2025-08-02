using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Check business rules before deletion
            // Check if user has active votes
            var hasVotes = await _context.Votes
                .AnyAsync(v => v.CreatedBy == request.UserId.ToString(), cancellationToken);
            
            if (hasVotes)
            {
                return Result<bool>.Failure("Cannot delete user with active votes. Please remove votes first.");
            }

            // Check if user has redeemed codes
            var hasRedeemedCodes = await _context.Codes
                .AnyAsync(c => c.RedeemerId != null && c.RedeemerId.Value == request.UserId, cancellationToken);
            
            if (hasRedeemedCodes)
            {
                return Result<bool>.Failure("Cannot delete user with redeemed codes. User data must be preserved for audit purposes.");
            }

            // Delete the user (this will cascade delete related UserVotes due to our configuration)
            await _userRepository.DeleteUserAsync(request.UserId);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}
