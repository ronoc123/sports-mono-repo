using FluentValidation;

namespace Application.Trivia.Commands.CreateTriviaSeries;

public class CreateTriviaSeriesCommandValidator : AbstractValidator<CreateTriviaSeriesCommand>
{
    public CreateTriviaSeriesCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("OrganizationId is required.");

        RuleFor(x => x.SeriesName)
            .NotEmpty().WithMessage("Series name is required.")
            .MaximumLength(200).WithMessage("Series name cannot exceed 200 characters.");
    }
}
