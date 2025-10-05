using FluentValidation;

namespace Application.Players.Commands.UpdatePlayer;

public class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    public UpdatePlayerCommandValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("Player ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Player name is required")
            .MaximumLength(200)
            .WithMessage("Player name must not exceed 200 characters");

        RuleFor(x => x.Position)
            .NotEmpty()
            .WithMessage("Position is required")
            .MaximumLength(100)
            .WithMessage("Position must not exceed 100 characters");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(16)
            .WithMessage("Player must be at least 16 years old")
            .LessThanOrEqualTo(50)
            .WithMessage("Player cannot be older than 50 years");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrlOrEmpty)
            .WithMessage("Image URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }

    private static bool BeValidUrlOrEmpty(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
