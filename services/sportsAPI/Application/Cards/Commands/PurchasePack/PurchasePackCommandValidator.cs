using FluentValidation;

namespace Application.Cards.Commands.PurchasePack;

public class PurchasePackCommandValidator : AbstractValidator<PurchasePackCommand>
{
    public PurchasePackCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.OrgId).NotEmpty().WithMessage("OrgId is required.");
        RuleFor(x => x.LeagueId).NotEmpty().WithMessage("LeagueId is required.");
    }
}
