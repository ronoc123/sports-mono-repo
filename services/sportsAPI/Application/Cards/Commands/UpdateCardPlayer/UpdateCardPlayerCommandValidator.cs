using FluentValidation;

namespace Application.Cards.Commands.UpdateCardPlayer;

public class UpdateCardPlayerCommandValidator : AbstractValidator<UpdateCardPlayerCommand>
{
    public UpdateCardPlayerCommandValidator()
    {
        RuleFor(x => x.CardPlayerId).NotEmpty();
        RuleFor(x => x.OrgId).NotEmpty();
        RuleFor(x => x.LeagueId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OverallRating)
            .InclusiveBetween(0, 99)
            .WithMessage("Overall rating must be between 0 and 99.");
    }
}
