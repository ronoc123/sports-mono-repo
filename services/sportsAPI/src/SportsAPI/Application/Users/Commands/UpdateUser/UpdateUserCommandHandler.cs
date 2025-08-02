using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the existing user
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            // Check if email is already taken by another user
            var existingUserWithEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id.Value != request.UserId, cancellationToken);
            
            if (existingUserWithEmail != null)
            {
                return Result<bool>.Failure("Email is already taken by another user");
            }

            // Check if username is already taken by another user
            var existingUserWithUsername = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.Id.Value != request.UserId, cancellationToken);
            
            if (existingUserWithUsername != null)
            {
                return Result<bool>.Failure("Username is already taken by another user");
            }

            // Update properties
            user.UpdateEmail(request.Email);
            user.UpdateUserName(request.UserName);

            await _userRepository.UpdateUserAsync(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update user: {ex.Message}");
        }
    }
}
