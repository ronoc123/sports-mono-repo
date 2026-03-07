using Contracts.Contracts;
using MediatR;

namespace Application.Trivia.Commands.AddTriviaQuestion;

public record AddTriviaQuestionCommand(
    Guid SeriesId,
    Guid OrganizationId,
    string QuestionText,
    List<string> AnswerOptions,
    string CorrectAnswer,
    int VoteRewardAmount
) : IRequest<ServiceResponse<AddTriviaQuestionResult>>;

public record AddTriviaQuestionResult(Guid QuestionId);
