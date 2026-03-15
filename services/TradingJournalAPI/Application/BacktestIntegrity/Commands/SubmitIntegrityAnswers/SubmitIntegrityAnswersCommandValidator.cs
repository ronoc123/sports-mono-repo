using FluentValidation;

namespace Application.BacktestIntegrity.Commands.SubmitIntegrityAnswers;

public class SubmitIntegrityAnswersCommandValidator : AbstractValidator<SubmitIntegrityAnswersCommand>
{
    public SubmitIntegrityAnswersCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Answers).NotNull().Must(a => a.Count == 6).WithMessage("Exactly 6 answers are required.");
        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(x => x.QuestionIndex).InclusiveBetween(1, 6);
        });
    }
}
