using FluentValidation;

namespace Application.Marketplace.Commands.CreateListing;

public sealed class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    private static readonly int[] ValidDurations = [1, 24, 48, 72];

    public CreateListingCommandValidator()
    {
        RuleFor(x => x.CardOwnerId).NotEmpty();
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.OrgId).NotEmpty();
        RuleFor(x => x.StartingBid).GreaterThan(0).WithMessage("Starting bid must be greater than zero.");
        RuleFor(x => x.DurationHours)
            .Must(d => ValidDurations.Contains(d))
            .WithMessage("Duration must be 1, 24, 48, or 72 hours.");
        RuleFor(x => x.BuyNowPrice)
            .GreaterThan(x => x.StartingBid)
            .When(x => x.BuyNowPrice.HasValue)
            .WithMessage("Buy now price must be greater than starting bid.");
    }
}
