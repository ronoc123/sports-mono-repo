using Contracts.Contracts;
using MediatR;

namespace Application.Poll.Commands.CreatePoll;

public record CreatePollCommand(
    Guid OrganizationId,
    string QuestionText,
    List<string> Options
) : IRequest<ServiceResponse<CreatePollResult>>;

public record CreatePollResult(
    Guid PollId,
    List<Guid> OptionIds
);
