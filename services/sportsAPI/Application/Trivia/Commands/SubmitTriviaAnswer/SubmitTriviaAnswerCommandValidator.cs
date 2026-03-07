using FluentValidation;

namespace Application.Trivia.Commands.SubmitTriviaAnswer;

public class SubmitTriviaAnswerCommandValidator : AbstractValidator<SubmitTriviaAnswerCommand>
{
    public SubmitTriviaAnswerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("OrganizationId is required.");
        RuleFor(x => x.TriviaQuestionId).NotEmpty().WithMessage("TriviaQuestionId is required.");
        RuleFor(x => x.SelectedAnswer).NotEmpty().WithMessage("SelectedAnswer is required.");
    }
}
