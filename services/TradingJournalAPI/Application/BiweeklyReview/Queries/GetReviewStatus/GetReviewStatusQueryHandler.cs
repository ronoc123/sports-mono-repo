using Application.Dto;
using Contracts.Contracts;
using Domain.BiweeklyReview;
using Domain.TradeJournal;
using MediatR;

namespace Application.BiweeklyReview.Queries.GetReviewStatus;

public sealed class GetReviewStatusQueryHandler
    : IRequestHandler<GetReviewStatusQuery, ServiceResponse<ReviewStatusDto>>
{
    private const int RequiredTrades = 8;
    private const int PeriodDays = 14;

    private readonly ITradeRepository _tradeRepo;
    private readonly IReviewSessionRepository _reviewRepo;

    public GetReviewStatusQueryHandler(ITradeRepository tradeRepo, IReviewSessionRepository reviewRepo)
    {
        _tradeRepo = tradeRepo;
        _reviewRepo = reviewRepo;
    }

    public async Task<ServiceResponse<ReviewStatusDto>> Handle(
        GetReviewStatusQuery request, CancellationToken cancellationToken)
    {
        var tradeCount = await _tradeRepo.CountRecentByUserIdAsync(request.UserId, PeriodDays, cancellationToken);
        var isReady = tradeCount >= RequiredTrades;

        Guid? sessionId = null;
        int daysUntilNext = 0;

        if (isReady)
        {
            var session = await _reviewRepo.GetLatestByUserIdAsync(request.UserId, cancellationToken);
            sessionId = session?.Id.Value;

            if (session is not null)
            {
                var daysSinceLastReview = (DateTime.UtcNow - session.CreatedAt!.Value).Days;
                daysUntilNext = Math.Max(0, PeriodDays - daysSinceLastReview);
            }
        }
        else
        {
            daysUntilNext = 0; // will show "X more trades needed" instead
        }

        return ServiceResponse.Ok(new ReviewStatusDto
        {
            IsReady = isReady,
            TradeCount = tradeCount,
            TradesRequired = RequiredTrades,
            DaysUntilNext = daysUntilNext,
            SessionId = sessionId
        }, "Success");
    }
}
