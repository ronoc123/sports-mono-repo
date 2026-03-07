using FluentValidation;

namespace Application.Poll.Commands.SubmitPollVote;

public class SubmitPollVoteCommandValidator : AbstractValidator<SubmitPollVoteCommand>
{
    public SubmitPollVoteCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("OrganizationId is required.");
        RuleFor(x => x.PollId).NotEmpty().WithMessage("PollId is required.");
        RuleFor(x => x.PollOptionId).NotEmpty().WithMessage("PollOptionId is required.");
    }
}
