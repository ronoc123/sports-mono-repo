using FluentValidation;

namespace Application.Marketplace.Commands.BuyNow;

public sealed class BuyNowCommandValidator : AbstractValidator<BuyNowCommand>
{
    public BuyNowCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.BuyerId).NotEmpty();
        RuleFor(x => x.LeagueId).NotEmpty();
    }
}
