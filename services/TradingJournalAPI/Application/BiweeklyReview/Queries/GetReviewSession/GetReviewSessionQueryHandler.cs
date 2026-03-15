using Application.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.BiweeklyReview;
using MediatR;

namespace Application.BiweeklyReview.Queries.GetReviewSession;

public sealed class GetReviewSessionQueryHandler
    : IRequestHandler<GetReviewSessionQuery, ServiceResponse<ReviewSessionDto>>
{
    private readonly IReviewSessionRepository _repo;

    public GetReviewSessionQueryHandler(IReviewSessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<ReviewSessionDto>> Handle(
        GetReviewSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByIdAsync(ReviewSessionId.From(request.SessionId), cancellationToken)
            ?? throw new EntityNotFoundException(nameof(ReviewSession), request.SessionId);

        return ServiceResponse.Ok(new ReviewSessionDto
        {
            Id = session.Id.Value,
            UserId = session.UserId,
            PeriodStart = session.PeriodStart,
            PeriodEnd = session.PeriodEnd,
            Messages = session.Messages.Select(m => new ReviewMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp
            }).ToList()
        }, "Success");
    }
}
