using FluentValidation;

namespace Application.TradeJournal.Commands.CreateTrade;

public class CreateTradeCommandValidator : AbstractValidator<CreateTradeCommand>
{
    public CreateTradeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
    }
}
