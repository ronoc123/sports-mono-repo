using FluentValidation;

namespace Application.Organizations.Commands.UpdateOrganization;

public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required")
            .MaximumLength(200)
            .WithMessage("Organization name must not exceed 200 characters");

        RuleFor(x => x.TeamId)
            .MaximumLength(50)
            .WithMessage("Team ID must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.TeamId));

        RuleFor(x => x.TeamName)
            .MaximumLength(200)
            .WithMessage("Team name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.TeamName));

        RuleFor(x => x.TeamShortName)
            .MaximumLength(10)
            .WithMessage("Team short name must not exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.TeamShortName));

        RuleFor(x => x.Sport)
            .MaximumLength(100)
            .WithMessage("Sport must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Sport));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.FormedYear)
            .GreaterThan(1800)
            .WithMessage("Formed year must be after 1800")
            .LessThanOrEqualTo(DateTime.Now.Year)
            .WithMessage("Formed year cannot be in the future")
            .When(x => x.FormedYear.HasValue);

        RuleFor(x => x.StadiumCapacity)
            .GreaterThan(0)
            .WithMessage("Stadium capacity must be greater than 0")
            .When(x => x.StadiumCapacity.HasValue);

        // URL validations
        RuleFor(x => x.BadgeUrl)
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Badge URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.BadgeUrl));

        RuleFor(x => x.LogoUrl)
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Logo URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.Website)
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Website must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.Website));
    }

    private static bool BeValidUrlOrEmpty(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
