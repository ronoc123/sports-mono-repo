using Application.Common.Interfaces;
using Application.Dto;
using Contracts.Contracts;
using Domain.AIConversation;
using Domain.TradeJournal;
using MediatR;

namespace Application.AIConversation.Commands.SendAIMessage;

public sealed class SendAIMessageCommandHandler : IRequestHandler<SendAIMessageCommand, ServiceResponse<AISessionDto>>
{
    private readonly IAISessionRepository _sessionRepo;
    private readonly IAIService _aiService;

    public SendAIMessageCommandHandler(IAISessionRepository sessionRepo, IAIService aiService)
    {
        _sessionRepo = sessionRepo;
        _aiService = aiService;
    }

    public async Task<ServiceResponse<AISessionDto>> Handle(SendAIMessageCommand request, CancellationToken cancellationToken)
    {
        AISession session;

        if (request.ExistingSessionId.HasValue)
        {
            session = await _sessionRepo.GetByIdAsync(
                AISessionId.From(request.ExistingSessionId.Value), cancellationToken)
                ?? AISession.Create(request.UserId, request.Context, request.LinkedEntityId);
        }
        else
        {
            var existing = await _sessionRepo.GetByLinkedEntityAsync(request.LinkedEntityId, cancellationToken);
            session = existing ?? AISession.Create(request.UserId, request.Context, request.LinkedEntityId);
        }

        var isNew = session.CreatedAt is null;

        var userMsg = session.AddUserMessage(request.UserMessage);

        // Build context and call AI
        string aiResponse;
        if (request.Context == AISessionContext.JournalEntry)
        {
            var phase = Enum.TryParse<TradePhase>(request.PhaseHint, out var p) ? p : TradePhase.PostTrade;
            var emotion = Enum.TryParse<EmotionalState>(request.EmotionalStateHint, out var e) ? e : EmotionalState.Calm;
            var ctx = new AIJournalContext(phase, emotion, request.IsEffortlessHint ?? true, request.NotesHint, null);
            aiResponse = await _aiService.GenerateJournalResponseAsync(session.Messages, ctx, cancellationToken);
        }
        else
        {
            var ctx = new AIIntegrityContext(0, "Integrity check", true, request.NotesHint);
            aiResponse = await _aiService.GenerateIntegrityResponseAsync(session.Messages, ctx, cancellationToken);
        }

        var assistantMsg = session.AddAssistantMessage(aiResponse);

        if (isNew)
            await _sessionRepo.AddAsync(session, cancellationToken);
        else
            _sessionRepo.AddMessages([userMsg, assistantMsg]);

        await _sessionRepo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(new AISessionDto
        {
            Id = session.Id.Value,
            UserId = session.UserId,
            Context = session.Context,
            LinkedEntityId = session.LinkedEntityId,
            Messages = session.Messages.Select(m => new AIMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp
            }).ToList()
        }, "Message sent.");
    }
}
