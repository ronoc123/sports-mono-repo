using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BiweeklyReview.Queries.GetReviewStatus;

public record GetReviewStatusQuery(Guid UserId) : IRequest<ServiceResponse<ReviewStatusDto>>;
