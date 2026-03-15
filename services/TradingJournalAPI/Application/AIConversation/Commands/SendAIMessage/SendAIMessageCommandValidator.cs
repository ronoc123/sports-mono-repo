using FluentValidation;

namespace Application.AIConversation.Commands.SendAIMessage;

public class SendAIMessageCommandValidator : AbstractValidator<SendAIMessageCommand>
{
    public SendAIMessageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LinkedEntityId).NotEmpty();
        RuleFor(x => x.UserMessage).NotEmpty().MaximumLength(4000);
    }
}
