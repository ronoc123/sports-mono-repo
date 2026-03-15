using Application.Common.Interfaces;
using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.BiweeklyReview;
using Domain.TradeJournal;
using MediatR;

namespace Application.BiweeklyReview.Commands.SendReviewMessage;

public sealed class SendReviewMessageCommandHandler
    : IRequestHandler<SendReviewMessageCommand, ServiceResponse<ReviewSessionDto>>
{
    private readonly IReviewSessionRepository _reviewRepo;
    private readonly ITradeRepository _tradeRepo;
    private readonly IAIService _aiService;

    public SendReviewMessageCommandHandler(
        IReviewSessionRepository reviewRepo,
        ITradeRepository tradeRepo,
        IAIService aiService)
    {
        _reviewRepo = reviewRepo;
        _tradeRepo = tradeRepo;
        _aiService = aiService;
    }

    public async Task<ServiceResponse<ReviewSessionDto>> Handle(
        SendReviewMessageCommand request, CancellationToken cancellationToken)
    {
        ReviewSession session;

        if (request.ExistingSessionId.HasValue)
        {
            session = await _reviewRepo.GetByIdAsync(
                ReviewSessionId.From(request.ExistingSessionId.Value), cancellationToken)
                ?? throw new EntityNotFoundException(nameof(ReviewSession), request.ExistingSessionId.Value);
        }
        else
        {
            var allTrades = await _tradeRepo.GetByUserIdAsync(request.UserId, cancellationToken);
            var recentCount = allTrades.Count(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-14));

            if (recentCount < 8)
                throw new DomainException($"Biweekly review requires at least 8 trades in the last 14 days. Current: {recentCount}.");

            var now = DateTime.UtcNow;
            session = ReviewSession.Create(request.UserId, now.AddDays(-14), now);
        }

        var isNew = session.CreatedAt is null;

        var userMsg = session.AddUserMessage(request.UserMessage);

        // Build review context
        var allTradesForContext = await _tradeRepo.GetByUserIdAsync(request.UserId, cancellationToken);
        var reviewTrades = allTradesForContext.Where(t => t.CreatedAt >= session.PeriodStart).ToList();
        var tradesSummary = BuildTradesSummary(reviewTrades);
        var ctx = new AIReviewContext(tradesSummary, "Identified from journal entries above.");

        var aiResponse = await _aiService.GenerateReviewResponseAsync(
            new List<Domain.AIConversation.AIMessage>(),
            ctx, cancellationToken);

        var assistantMsg = session.AddAssistantMessage(aiResponse);

        if (isNew)
            await _reviewRepo.AddAsync(session, cancellationToken);
        else
            _reviewRepo.AddMessages([userMsg, assistantMsg]);

        await _reviewRepo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(ToDto(session), "Review message sent.");
    }

    private static string BuildTradesSummary(List<Trade> trades)
    {
        if (!trades.Any()) return "No trades in review period.";

        var lines = trades.Select(t =>
        {
            var entries = t.JournalEntries
                .Select(e => $"{e.Phase}: {e.EmotionalState}, effortless={e.IsEffortless}")
                .ToList();
            return $"- {t.Symbol} ({t.CreatedAt:yyyy-MM-dd}): [{string.Join(" | ", entries)}]";
        });

        return string.Join("\n", lines);
    }

    private static ReviewSessionDto ToDto(ReviewSession s) => new()
    {
        Id = s.Id.Value,
        UserId = s.UserId,
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        Messages = s.Messages.Select(m => new ReviewMessageDto
        {
            Id = m.Id,
            Role = m.Role,
            Content = m.Content,
            Timestamp = m.Timestamp
        }).ToList()
    };
}
