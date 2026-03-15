using Application.Dto;
using Contracts.Contracts;
using Domain.AIConversation;
using MediatR;

namespace Application.AIConversation.Commands.SendAIMessage;

public record SendAIMessageCommand(
    Guid UserId,
    Guid? ExistingSessionId,
    AISessionContext Context,
    Guid LinkedEntityId,
    string UserMessage,
    // Context hints for AI prompt building
    string? PhaseHint,
    string? EmotionalStateHint,
    bool? IsEffortlessHint,
    string? NotesHint
) : IRequest<ServiceResponse<AISessionDto>>;
