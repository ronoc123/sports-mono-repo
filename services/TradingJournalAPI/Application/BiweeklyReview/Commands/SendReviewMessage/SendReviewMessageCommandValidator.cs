using FluentValidation;

namespace Application.BiweeklyReview.Commands.SendReviewMessage;

public class SendReviewMessageCommandValidator : AbstractValidator<SendReviewMessageCommand>
{
    public SendReviewMessageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserMessage).NotEmpty().MaximumLength(4000);
    }
}
