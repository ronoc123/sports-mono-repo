using FluentValidation;

namespace Application.Poll.Commands.CreatePoll;

public class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("OrganizationId is required.");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(1000).WithMessage("Question text cannot exceed 1000 characters.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("At least two options are required.")
            .Must(opts => opts.Count >= 2).WithMessage("At least two options are required.");
    }
}
