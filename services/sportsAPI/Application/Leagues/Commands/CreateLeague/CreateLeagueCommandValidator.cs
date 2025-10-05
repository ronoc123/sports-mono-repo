using FluentValidation;

namespace Application.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
{
    public CreateLeagueCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("League name is required")
            .MaximumLength(100)
            .WithMessage("League name must not exceed 100 characters");
    }
}
