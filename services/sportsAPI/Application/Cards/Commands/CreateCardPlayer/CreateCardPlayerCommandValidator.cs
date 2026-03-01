using FluentValidation;

namespace Application.Cards.Commands.CreateCardPlayer;

public class CreateCardPlayerCommandValidator : AbstractValidator<CreateCardPlayerCommand>
{
    public CreateCardPlayerCommandValidator()
    {
        RuleFor(x => x.LeagueId).NotEmpty().WithMessage("LeagueId is required.");
        RuleFor(x => x.OrgId).NotEmpty().WithMessage("OrgId is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OverallRating)
            .InclusiveBetween(0, 99)
            .WithMessage("Overall rating must be between 0 and 99.");
    }
}
