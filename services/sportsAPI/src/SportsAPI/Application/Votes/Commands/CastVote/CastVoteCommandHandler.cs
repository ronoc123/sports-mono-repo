using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using Domain.User.Entities;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Votes.Commands.CastVote;

public class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public CastVoteCommandHandler(IApplicationDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<Result<Guid>> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the user
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return Result<Guid>.Failure("User not found");
            }

            // Check if user has enough votes for this organization
            var organizationId = OrganizationId.Of(request.OrganizationId);
            var userVotes = user.VotesAvailable.FirstOrDefault(v => v.OrganizationId.Value == request.OrganizationId);
            
            if (userVotes == null || userVotes.VotesRemaining < request.VotesSpent)
            {
                return Result<Guid>.Failure("Insufficient votes available");
            }

            // Create the vote
            var vote = Vote.Create();

            // Use the votes
            for (int i = 0; i < request.VotesSpent; i++)
            {
                user.UseVoteForOrganization(organizationId);
            }

            // Add the vote to the user
            user.AddVote(vote);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(vote.Id.Value);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to cast vote: {ex.Message}");
        }
    }
}
