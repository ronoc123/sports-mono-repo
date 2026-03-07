using Contracts.Contracts;
using MediatR;

namespace Application.Poll.Commands.SubmitPollVote;

public record SubmitPollVoteCommand(
    Guid UserId,
    Guid OrganizationId,
    Guid PollId,
    Guid PollOptionId
) : IRequest<ServiceResponse<SubmitPollVoteResult>>;

public record SubmitPollVoteResult(
    Guid VotedOptionId,
    List<PollVoteOptionResult> Options
);

public record PollVoteOptionResult(
    Guid PollOptionId,
    string OptionText,
    int VoteCount
);
