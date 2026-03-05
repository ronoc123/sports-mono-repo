using FluentValidation;

namespace Application.Marketplace.Commands.PlaceBid;

public sealed class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.BidderId).NotEmpty();
        RuleFor(x => x.LeagueId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Bid amount must be greater than zero.");
    }
}
