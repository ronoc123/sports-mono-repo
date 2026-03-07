using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Commands.SubmitTriviaAnswer;

public record SubmitTriviaAnswerCommand(
    Guid UserId,
    Guid OrganizationId,
    Guid TriviaQuestionId,
    string SelectedAnswer
) : IRequest<ServiceResponse<SubmitTriviaAnswerResult>>;

public record SubmitTriviaAnswerResult(
    bool IsCorrect,
    int VotesEarned,
    string CorrectAnswer
);
