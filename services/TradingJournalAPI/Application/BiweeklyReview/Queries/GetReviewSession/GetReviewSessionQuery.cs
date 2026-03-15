using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BiweeklyReview.Queries.GetReviewSession;

public record GetReviewSessionQuery(Guid SessionId) : IRequest<ServiceResponse<ReviewSessionDto>>;
