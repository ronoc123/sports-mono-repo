using FluentValidation;

namespace Application.Trivia.Commands.AddTriviaQuestion;

public class AddTriviaQuestionCommandValidator : AbstractValidator<AddTriviaQuestionCommand>
{
    public AddTriviaQuestionCommandValidator()
    {
        RuleFor(x => x.SeriesId).NotEmpty().WithMessage("SeriesId is required.");
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("OrganizationId is required.");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(1000).WithMessage("Question text cannot exceed 1000 characters.");

        RuleFor(x => x.AnswerOptions)
            .NotEmpty().WithMessage("At least two answer options are required.")
            .Must(opts => opts.Count >= 2).WithMessage("At least two answer options are required.");

        RuleFor(x => x.CorrectAnswer)
            .NotEmpty().WithMessage("Correct answer is required.");

        RuleFor(x => x.VoteRewardAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Vote reward cannot be negative.");
    }
}
