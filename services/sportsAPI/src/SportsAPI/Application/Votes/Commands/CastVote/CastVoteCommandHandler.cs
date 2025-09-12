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
        return null;
    }
}
