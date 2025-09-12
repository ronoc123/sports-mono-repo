using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Codes.Commands.RedeemCode;

public class RedeemCodeCommandHandler : IRequestHandler<RedeemCodeCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public RedeemCodeCommandHandler(IApplicationDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(RedeemCodeCommand request, CancellationToken cancellationToken)
    {
        return null;
    }
}
