using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Poll.Commands.ArchivePoll;

public sealed class ArchivePollCommandHandler
    : IRequestHandler<ArchivePollCommand, ServiceResponse<bool>>
{
    private readonly IPollRepository _repo;

    public ArchivePollCommandHandler(IPollRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<bool>> Handle(
        ArchivePollCommand request, CancellationToken ct)
    {
        var pollId = PollId.Of(request.PollId);
        var poll = await _repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(p => p.Id == pollId, ct)
            ?? throw new DomainException($"Poll '{request.PollId}' not found.");

        poll.Archive();
        await _repo.SaveChangesAsync(ct);

        return ServiceResponse.Ok(true, "Poll archived.");
    }
}
