using FluentValidation;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public class CreatePlayerOptionCommandValidator : AbstractValidator<CreatePlayerOptionCommand>
{
    public CreatePlayerOptionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("Player ID is required");

        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithMessage("Organization ID is required");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1))
            .WithMessage("Expiry date cannot be more than 1 year in the future")
            .When(x => x.ExpiresAt.HasValue);
    }
}
