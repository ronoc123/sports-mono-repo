using FluentValidation;

namespace Application.Leagues.Commands.UpdateLeague;

public class UpdateLeagueCommandValidator : AbstractValidator<UpdateLeagueCommand>
{
    public UpdateLeagueCommandValidator()
    {
        RuleFor(x => x.LeagueId)
            .NotEmpty()
            .WithMessage("League ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("League name is required")
            .MaximumLength(200)
            .WithMessage("League name must not exceed 200 characters");
    }
}
