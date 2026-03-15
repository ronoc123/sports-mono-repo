using FluentValidation;

namespace Application.TradeJournal.Commands.UpsertJournalEntry;

public class UpsertJournalEntryCommandValidator : AbstractValidator<UpsertJournalEntryCommand>
{
    public UpsertJournalEntryCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TradeId).NotEmpty();
        RuleFor(x => x.FreeFormNotes).MaximumLength(5000).When(x => x.FreeFormNotes is not null);
    }
}
