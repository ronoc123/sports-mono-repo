using FluentValidation;

namespace Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required")
            .MaximumLength(100)
            .WithMessage("Organization name must not exceed 100 characters");

        RuleFor(x => x.LeagueId)
            .NotEmpty()
            .WithMessage("League ID is required");

        RuleFor(x => x.FormedYear)
            .GreaterThan(1800)
            .LessThanOrEqualTo(DateTime.Now.Year)
            .When(x => x.FormedYear.HasValue)
            .WithMessage("Formed year must be between 1800 and current year");

        RuleFor(x => x.StadiumCapacity)
            .GreaterThan(0)
            .When(x => x.StadiumCapacity.HasValue)
            .WithMessage("Stadium capacity must be greater than 0");
    }
}
