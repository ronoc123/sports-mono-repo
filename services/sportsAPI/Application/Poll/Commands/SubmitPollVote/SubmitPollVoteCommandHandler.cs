using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Enums;
using Domain.Poll;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Poll.Commands.SubmitPollVote;

public sealed class SubmitPollVoteCommandHandler
    : IRequestHandler<SubmitPollVoteCommand, ServiceResponse<SubmitPollVoteResult>>
{
    private readonly IPollRepository _pollRepo;
    private readonly IApplicationDbContext _db;

    public SubmitPollVoteCommandHandler(IPollRepository pollRepo, IApplicationDbContext db)
    {
        _pollRepo = pollRepo;
        _db = db;
    }

    public async Task<ServiceResponse<SubmitPollVoteResult>> Handle(
        SubmitPollVoteCommand request, CancellationToken ct)
    {
        var pollId = PollId.Of(request.PollId);
        var optionId = PollOptionId.Of(request.PollOptionId);

        // 1. Load the poll with options (use query to avoid FindAsync value-converter mismatch)
        var poll = await _pollRepo.Query(asNoTracking: false)
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == pollId, ct)
            ?? throw new DomainException($"Poll '{request.PollId}' not found.");

        if (poll.Status != PollStatus.Active)
            throw new DomainException("This poll is not currently active.");

        // Validate option belongs to the poll
        poll.GetOption(optionId); // throws if not found

        // 2. Idempotency guard
        var alreadyVoted = await _db.PollVotes
            .AnyAsync(v => v.UserId == request.UserId && v.PollId == pollId, ct);

        if (alreadyVoted)
            throw new DomainException("You have already voted on this poll.");

        // 3. Record the vote and increment option count
        var vote = PollVote.Record(pollId, optionId, request.UserId);
        var option = poll.GetOption(optionId);
        option.IncrementVoteCount();

        await _db.PollVotes.AddAsync(vote, ct);
        await _db.SaveChangesAsync(ct);

        // 4. Build updated option counts from in-memory state (option.VoteCount was incremented)
        var optionResults = poll.Options
            .OrderBy(o => o.SortOrder)
            .Select(o =>
            {
                // Re-read from DB for accurate counts (other concurrent votes)
                var dbCount = _db.PollVotes
                    .Count(v => v.PollOptionId == o.Id);
                return new PollVoteOptionResult(o.Id.Value, o.Text, dbCount);
            })
            .ToList();

        return ServiceResponse.Ok(
            new SubmitPollVoteResult(request.PollOptionId, optionResults),
            "Vote recorded.");
    }
}
