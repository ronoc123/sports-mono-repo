using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.BiweeklyReview.Commands.SendReviewMessage;

public record SendReviewMessageCommand(
    Guid UserId,
    Guid? ExistingSessionId,
    string UserMessage
) : IRequest<ServiceResponse<ReviewSessionDto>>;
